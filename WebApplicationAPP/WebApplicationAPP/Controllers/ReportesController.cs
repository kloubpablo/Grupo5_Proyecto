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

        private bool EsAdmin()
        {
            return HttpContext.Session
                .GetString("Rol") == "Administrador";
        }

       
        public IActionResult Index()
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            int citasHoy = _context.Citas
                .Count(c =>
                    c.Fecha ==
                    DateOnly.FromDateTime(DateTime.Now));

            decimal ingresosHoy = _context.Pagos
                .Where(p =>
                    p.Fecha ==
                    DateOnly.FromDateTime(DateTime.Now))
                .Sum(p => (decimal?)p.Monto) ?? 0;

            
            int clientesFrecuentes = _context.Clientes
                .Count();

            ViewBag.CitasHoy = citasHoy;
            ViewBag.IngresosHoy = ingresosHoy;
            ViewBag.ClientesFrecuentes = clientesFrecuentes;

            return View();
        }

        
        public IActionResult PorFecha()
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View();
        }

      
        [HttpPost]
        public IActionResult PorFecha(DateOnly fecha)
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            
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