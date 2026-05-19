using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class DashboardController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public DashboardController(YampiBarbershopContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 📅 Citas de hoy
            var citasHoy = _context.Citas
                .Count(c => c.Fecha == DateOnly.FromDateTime(DateTime.Now));

            // 👤 Clientes atendidos
            var clientesAtendidos = _context.Atencions
                .Count(a => a.Estado == "Finalizado");

            // 💰 Ingresos de hoy
            decimal ingresosHoy = 0;

            if (_context.Pagos.Any())
            {
                ingresosHoy = _context.Pagos
                    .Where(p => p.Fecha == DateOnly.FromDateTime(DateTime.Now))
                    .Sum(p => (decimal?)p.Monto) ?? 0;
            }

            // ViewBag dinámico
            ViewBag.CitasHoy = citasHoy;
            ViewBag.IngresosHoy = ingresosHoy;
            ViewBag.ClientesAtendidos = clientesAtendidos;

            // 💈 Barberos reales desde SQL Server
            var barberos = _context.Usuarios
                .Where(u => u.IdRol == 2)
                .ToList();

            return View(barberos);
        }
    }
}