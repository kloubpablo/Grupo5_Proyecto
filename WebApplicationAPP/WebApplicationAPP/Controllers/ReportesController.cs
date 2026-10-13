using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;
using Microsoft.EntityFrameworkCore;


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

        //DASHBOARD DE REPORTES

        public IActionResult Index()
        {
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