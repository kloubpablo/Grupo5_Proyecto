using System.Data;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Services;

public sealed record ResultadoCobro(bool Ok, string Mensaje, int? IdPago = null);

// Ambos formularios (HTML y AJAX) llaman exactamente la misma operacion.
// Pago y finalizacion se confirman o revierten juntos.
public static class CobroAtencionService
{
    public static async Task<ResultadoCobro> CobrarAsync(YampiBarbershopContext db,
        int idAtencion, decimal monto, string metodo, Usuario usuario)
    {
        if (idAtencion <= 0 || monto <= 0 || monto > 99999999.99m ||
            decimal.Round(monto, 2) != monto)
            return new(false, "Ingresa un monto positivo con maximo dos decimales.");
        if (metodo != "Efectivo" && metodo != "Tarjeta")
            return new(false, "Selecciona Efectivo o Tarjeta.");

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var atencion = await db.Atencions
            .Include(a => a.IdCitaNavigation)
            .FirstOrDefaultAsync(a => a.IdAtencion == idAtencion);
        if (atencion == null) return new(false, "La atencion no existe.");
        if (!AgendaBarberoService.PuedeGestionar(usuario, atencion.IdBarbero))
            return new(false, "No puedes cobrar una atencion de otro barbero.");
        if (atencion.Estado != "En servicio")
            return new(false, "La atencion no esta en servicio o ya finalizo.");
        if (await db.Pagos.AnyAsync(p => p.IdAtencion == idAtencion))
            return new(false, "Esta atencion ya tiene un pago registrado.");
        if (atencion.IdCitaNavigation != null && atencion.IdCitaNavigation.Estado != "Activa")
            return new(false, "La cita no tiene un estado compatible con la finalizacion.");

        var ahora = AgendaBarberoService.AhoraCostaRica();
        var pago = new Pago
        {
            IdAtencion = atencion.IdAtencion,
            IdCliente = atencion.IdCliente,
            Monto = monto,
            Metodo = metodo,
            Fecha = DateOnly.FromDateTime(ahora),
            Cerrado = false
        };
        db.Pagos.Add(pago);
        atencion.Estado = "Finalizado";
        atencion.HoraFin = TimeOnly.FromDateTime(ahora);
        if (atencion.IdCitaNavigation != null)
            atencion.IdCitaNavigation.Estado = "Finalizada";

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return new(true, "Pago registrado y atencion finalizada correctamente.", pago.IdPago);
    }
}
