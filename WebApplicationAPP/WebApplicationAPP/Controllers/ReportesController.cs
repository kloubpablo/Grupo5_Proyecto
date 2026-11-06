using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;
using Microsoft.EntityFrameworkCore;


namespace WebApplicationAPP.Controllers
{
    public class ReportesController : Controller
    {
        private readonly YampiBarbershopContext _context;
        // Autorizacion real de administracion: nunca confiar solo en un boton o en el Rol de Session.
        private bool EsAdministradorActual()
        {
            int? id = HttpContext.Session.GetInt32("IdUsuario");
            return id.HasValue && _context.Usuarios.AsNoTracking()
                .Any(u => u.IdUsuario == id.Value && u.Estado && !u.ContraTemp &&
                    u.IdRolNavigation.Estado && u.IdRolNavigation.Nombre == "Administrador");
        }


        public ReportesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private bool TienePermiso(string permiso) => EsAdministradorActual();

        //DASHBOARD DE REPORTES

        public IActionResult Index()
        {
            if (!EsAdministradorActual()) return StatusCode(403);
            // Mantener aqui tu validacion de permisos actual

            var pagos = _context.Pagos
                .Include(p => p.IdClienteNavigation)
                .AsNoTracking()
                .OrderByDescending(p => p.Fecha)
                .ToList();

            // Las fechas de las citas permiten calcular
            // cuantos registros hubo en cada intervalo.

            var fechasCitas = _context.Citas
                .AsNoTracking()
                .Select(c => c.Fecha)
                .ToList()
                .Select(fecha => fecha.ToString("yyyy-MM-dd"))
                .ToList();

            ViewBag.FechasCitas = fechasCitas;

            return View(pagos);
        }


        //REPORTE POR FECHA (GET)
        public IActionResult PorFecha()
        {
            if (!TienePermiso("Reportes/Index"))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        //REPORTE POR FECHA (POST)
        [HttpPost]
        public IActionResult PorFecha(DateOnly fecha)
        {
            if (!TienePermiso("Reportes/Index"))
                return RedirectToAction("Index", "Dashboard");

            decimal total = _context.Pagos
                .Where(p => p.Fecha == fecha)
                .Sum(p => (decimal?)p.Monto) ?? 0;

            int citas = _context.Citas
                .Count(c => c.Fecha == fecha);

            var pagos = _context.Pagos
                .Where(p => p.Fecha == fecha)
                .OrderByDescending(p => p.IdPago)
                .ToList();

            ViewBag.Fecha = fecha;
            ViewBag.Total = total;
            ViewBag.Citas = citas;
            ViewBag.Pagos = pagos;

            return View();
        }


        //CLIENTES FRECUENTES
        public IActionResult ClientesFrecuentes()
        {
            if (!TienePermiso("Reportes/Index"))
                return RedirectToAction("Index", "Dashboard");

            var clientes = _context.VwClientesFrecuentes
                .OrderByDescending(c => c.TotalCitas)
                .ToList();

            return View(clientes);
        }

    }
}