using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class ReportesController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public ReportesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // REPORTE GENERAL
        public IActionResult Index()
        {
            // 📅 Citas de hoy
            int citasHoy = _context.Citas
                .Count(c => c.Fecha == DateOnly.FromDateTime(DateTime.Now));

            // 💰 Ingresos de hoy
            decimal ingresosHoy = _context.Pagos
                .Where(p => p.Fecha == DateOnly.FromDateTime(DateTime.Now))
                .Sum(p => (decimal?)p.Monto) ?? 0;

            // 👤 Clientes frecuentes
            int clientesFrecuentes = _context.Clientes
                .Count();

            ViewBag.CitasHoy = citasHoy;
            ViewBag.IngresosHoy = ingresosHoy;
            ViewBag.ClientesFrecuentes = clientesFrecuentes;

            return View();
        }

        // REPORTE POR FECHA (GET)
        public IActionResult PorFecha()
        {
            return View();
        }

        // REPORTE POR FECHA (POST)
        [HttpPost]
        public IActionResult PorFecha(DateOnly fecha)
        {
            // Total pagos del día
            decimal total = _context.Pagos
                .Where(p => p.Fecha == fecha)
                .Sum(p => (decimal?)p.Monto) ?? 0;

            // Cantidad de citas
            int citas = _context.Citas
                .Count(c => c.Fecha == fecha);

            ViewBag.Fecha = fecha;
            ViewBag.Total = total;
            ViewBag.Citas = citas;

            return View();
        }
    }
}