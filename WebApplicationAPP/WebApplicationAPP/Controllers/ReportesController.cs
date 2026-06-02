using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;

namespace WebApplicationAPP.Controllers
{
    public class ReportesController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public ReportesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
                return false;

            return PermisosHelper.TienePermiso(_context, rol, permiso);
        }

        // 🔥 DASHBOARD DE REPORTES
        public IActionResult Index()
        {
            if (!TienePermiso("Reportes/Index"))
                return RedirectToAction("Index", "Dashboard");

            int citasHoy = _context.Citas
                .Count(c => c.Fecha == DateOnly.FromDateTime(DateTime.Now));

            decimal ingresosHoy = _context.Pagos
                .Where(p => p.Fecha == DateOnly.FromDateTime(DateTime.Now))
                .Sum(p => (decimal?)p.Monto) ?? 0;

            int clientesFrecuentes = _context.Clientes.Count();

            ViewBag.CitasHoy = citasHoy;
            ViewBag.IngresosHoy = ingresosHoy;
            ViewBag.ClientesFrecuentes = clientesFrecuentes;

            return View();
        }

        // 🔥 REPORTE POR FECHA (GET)
        public IActionResult PorFecha()
        {
            if (!TienePermiso("Reportes/Index"))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // 🔥 REPORTE POR FECHA (POST)
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

            ViewBag.Fecha = fecha;
            ViewBag.Total = total;
            ViewBag.Citas = citas;

            return View();
        }
    }
}