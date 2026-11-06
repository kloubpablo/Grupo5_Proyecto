using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Services;

// Una sola fuente de reglas para reservas online y clientes presenciales.
// Slot actual de 30 minutos: ampliar a reservas por intervalo al incorporar duraciones variables.
public static class AgendaBarberoService
{
    public const int MinutosPorTurno = 30;
    public static readonly TimeOnly Apertura = new(7, 0);
    public static readonly TimeOnly UltimoInicio = new(20, 30);

    public static DateTime AhoraCostaRica()
    {
        TimeZoneInfo zona;
        try { zona = TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica"); }
        catch (TimeZoneNotFoundException)
        {
            zona = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
    }

    public static bool EsHorarioValido(DateOnly fecha, TimeOnly hora, bool ingresoPresencial = false)
    {
        var ahora = AhoraCostaRica();
        var hoy = DateOnly.FromDateTime(ahora);
        if (fecha == default || fecha < hoy || fecha.DayOfWeek == DayOfWeek.Sunday)
            return false;
        if (hora < Apertura || hora > UltimoInicio || hora.Second != 0 ||
            (hora.Minute != 0 && hora.Minute != 30))
            return false;

        var inicio = fecha.ToDateTime(hora);
        // Un ingreso presencial puede tomar el espacio actual si aun no termina.
        return fecha != hoy || (ingresoPresencial
            ? inicio.AddMinutes(MinutosPorTurno) > ahora
            : inicio > ahora);
    }

    public static Usuario? BuscarBarbero(YampiBarbershopContext db, int idBarbero) =>
        db.Usuarios.Include(u => u.IdRolNavigation)
          .FirstOrDefault(u => u.IdUsuario == idBarbero && u.Estado && !u.ContraTemp &&
              u.IdRolNavigation.Estado && u.IdRolNavigation.Nombre == "Barbero");

    // Incluye registros antiguos pendientes de vincular por ID.
    // Una coincidencia de nombre entre varias cuentas debe resolverse ANTES
    // de habilitar nuevas reservas para esos barberos.
    public static bool Ocupado(YampiBarbershopContext db, int idBarbero,
        string nombreBarbero, DateOnly fecha, TimeOnly hora, int? excluirCita = null)
    {
        return db.Citas.Any(c => c.Fecha == fecha && c.Hora == hora &&
            (c.Estado == "Activa" || c.Estado == "Bloqueada") &&
            (!excluirCita.HasValue || c.IdCita != excluirCita.Value) &&
            (c.IdBarbero == idBarbero ||
                (c.IdBarbero == null && c.Barbero == nombreBarbero)));
    }

    public static bool PuedeGestionar(Usuario usuario, int? idBarbero) =>
        usuario.IdRolNavigation.Nombre == "Administrador" ||
        (usuario.IdRolNavigation.Nombre == "Barbero" && idBarbero.HasValue &&
         usuario.IdUsuario == idBarbero.Value);

    public static bool EsAdministrador(Usuario u) => u.IdRolNavigation.Nombre == "Administrador";
}
