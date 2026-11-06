using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Helpers;
using WebApplicationAPP.Models;
using WebApplicationAPP.Services;

namespace WebApplicationAPP.Controllers;

public class PagosController : Controller
{
    private readonly YampiBarbershopContext _db;
    public PagosController(YampiBarbershopContext db) => _db = db;

    private Usuario? Actual()
    {
        int? id = HttpContext.Session.GetInt32("IdUsuario");
        if (!id.HasValue) return null;
        return _db.Usuarios.AsNoTracking().Include(u => u.IdRolNavigation)
            .FirstOrDefault(u => u.IdUsuario == id && u.Estado && !u.ContraTemp &&
                 u.IdRolNavigation.Estado &&
                 (u.IdRolNavigation.Nombre == "Administrador" || u.IdRolNavigation.Nombre == "Barbero"));
    }

    private bool Permiso(Usuario u, string clave) => AgendaBarberoService.EsAdministrador(u) ||
        PermisosHelper.TienePermiso(_db, u, clave);

    private IActionResult SinAcceso(Usuario? u) =>
        u == null ? RedirectToAction("Index", "Seguridad") : StatusCode(403);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var u = Actual();
        if (u == null) return SinAcceso(u);
        if (!Permiso(u, "Pagos/Index")) return StatusCode(403);

        bool esAdmin = AgendaBarberoService.EsAdministrador(u);
        IQueryable<Pago> consulta = _db.Pagos.AsNoTracking()
            .Include(p => p.IdClienteNavigation);

        if (!esAdmin)
        {
            if (u.IdRolNavigation.Nombre != "Barbero") return StatusCode(403);
            // Filtro aplicado ANTES de materializar la lista. Nunca traer toda la caja.
            // Los pagos historicos/manuales sin IdAtencion NO son atribuibles por nombre.
            int idBarbero = u.IdUsuario;
            consulta = consulta.Where(p => p.IdAtencion.HasValue &&
                p.Atencion != null && p.Atencion.IdBarbero == idBarbero);
        }

        ViewBag.EsBarbero = !esAdmin;
        var pagos = await consulta.OrderByDescending(p => p.Fecha)
            .ThenByDescending(p => p.IdPago).ToListAsync();
        return View(pagos);
    }

    [HttpGet]
    public IActionResult Registrar(int? idAtencion)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Pagos/Crear")) return SinAcceso(u);
        if (!idAtencion.HasValue && !AgendaBarberoService.EsAdministrador(u))
            return StatusCode(403);
        ViewBag.IdAtencion = idAtencion;
        if (!idAtencion.HasValue)
            ViewBag.Clientes = _db.Clientes.AsNoTracking().OrderBy(c => c.Nombre).ToList();
        if (idAtencion.HasValue)
        {
            var a = _db.Atencions.AsNoTracking().Include(x => x.IdClienteNavigation)
                .FirstOrDefault(x => x.IdAtencion == idAtencion.Value);
            if (a == null) return NotFound();
            if (!AgendaBarberoService.PuedeGestionar(u, a.IdBarbero)) return StatusCode(403);
            if (a.Estado != "En servicio") return BadRequest("La atencion no esta en servicio.");
            ViewBag.ClienteAtencion = a.IdClienteNavigation.Nombre;
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int? idCliente, decimal monto, string? metodo, int? idAtencion)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Pagos/Crear")) return SinAcceso(u);

        if (idAtencion.HasValue)
        {
            try
            {
                var resultado = await CobroAtencionService.CobrarAsync(_db,
                    idAtencion.Value, monto, metodo ?? "", u);
                if (resultado.Ok)
                {
                    TempData["ExitoAtencion"] = resultado.Mensaje;
                    return RedirectToAction("Index", "Atencion");
                }
                ViewBag.Error = resultado.Mensaje;
            }
            catch (DbUpdateException)
            {
                ViewBag.Error = "La atencion fue modificada o ya tiene un pago. Recarga la pagina.";
            }
            ViewBag.IdAtencion = idAtencion;
            // No confiar en cliente del POST; solo se presenta informacion desde la BD.
            ViewBag.ClienteAtencion = _db.Atencions.AsNoTracking()
                .Where(a => a.IdAtencion == idAtencion.Value)
                .Select(a => a.IdClienteNavigation.Nombre).FirstOrDefault();
            return View();
        }

        if (!AgendaBarberoService.EsAdministrador(u)) return StatusCode(403);
        if (!idCliente.HasValue || !_db.Clientes.Any(c => c.IdCliente == idCliente.Value) ||
            monto <= 0 || monto > 99999999.99m || decimal.Round(monto, 2) != monto ||
            (metodo != "Efectivo" && metodo != "Tarjeta"))
        {
            ViewBag.Clientes = _db.Clientes.AsNoTracking().OrderBy(c => c.Nombre).ToList();
            ViewBag.Error = "Selecciona un cliente existente, un monto y un metodo valido.";
            return View();
        }
        // Nunca se asocia a un cliente solo porque coincida su nombre.
        _db.Pagos.Add(new Pago
        {
            IdCliente = idCliente.Value,
            Monto = monto,
            Metodo = metodo!,
            Fecha = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica())
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarAjax(int idAtencion, decimal monto, string? metodo)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Pagos/Crear"))
            return StatusCode(403, new { ok = false, mensaje = "Acceso denegado." });
        try
        {
            var resultado = await CobroAtencionService.CobrarAsync(_db,
                idAtencion, monto, metodo ?? "", u);
            if (!resultado.Ok) return BadRequest(new { ok = false, mensaje = resultado.Mensaje });
            return Json(new { ok = true, mensaje = resultado.Mensaje, idPago = resultado.IdPago });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { ok = false, mensaje = "La atencion ya fue cobrada o modificada." });
        }
    }

    [HttpGet]
    public IActionResult Cierre()
    {
        var u = Actual();
        if (u == null || !AgendaBarberoService.EsAdministrador(u)) return SinAcceso(u);
        var ahora = AgendaBarberoService.AhoraCostaRica();
        var hoy = DateOnly.FromDateTime(ahora);
        var pagosHoy = _db.Pagos.AsNoTracking().Where(p => p.Fecha == hoy && !p.Cerrado).ToList();
        ViewBag.Fecha = ahora.ToString("dd/MM/yyyy");
        ViewBag.Total = pagosHoy.Sum(p => p.Monto);
        ViewBag.CantidadPagos = pagosHoy.Count;
        ViewBag.MetodoMasUsado = pagosHoy.GroupBy(p => p.Metodo)
            .OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "Sin registros";
        ViewBag.TotalEfectivo = pagosHoy.Where(p => p.Metodo == "Efectivo").Sum(p => p.Monto);
        ViewBag.TotalTarjeta = pagosHoy.Where(p => p.Metodo == "Tarjeta").Sum(p => p.Monto);
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AplicarCierre()
    {
        var u = Actual();
        if (u == null || !AgendaBarberoService.EsAdministrador(u)) return SinAcceso(u);
        var hoy = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica());
        var pagos = _db.Pagos.Where(p => p.Fecha == hoy && !p.Cerrado).ToList();
        foreach (var pago in pagos) pago.Cerrado = true;
        _db.SaveChanges();
        TempData["ExitoCaja"] = "Cierre de caja realizado.";
        return RedirectToAction(nameof(Index));
    }
}
