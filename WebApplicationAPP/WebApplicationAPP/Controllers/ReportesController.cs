using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class ReportesController : Controller
    {
        // REPORTE GENERAL
        public IActionResult Index()
        {
            // 🔹 Datos simulados
            ViewBag.CitasHoy = 10;
            ViewBag.IngresosHoy = 50000;
            ViewBag.ClientesFrecuentes = 5;

            return View();
        }

        // REPORTE POR FECHA
        public IActionResult PorFecha()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PorFecha(string fecha)
        {
            if (string.IsNullOrEmpty(fecha))
            {
                ViewBag.Error = "Seleccione una fecha";
                return View();
            }

            // Simulación
            ViewBag.Resultado = "Reporte generado para: " + fecha;
            ViewBag.Total = 30000;

            return View();
        }
    }
}