using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // 🔹 Datos simulados (luego podrían venir de BD)
            ViewBag.CitasHoy = 12;
            ViewBag.IngresosHoy = 75000;
            ViewBag.ClientesAtendidos = 9;

            // Estado de barberos (simulado)
            ViewBag.Barberos = new List<dynamic>
            {
                new { nombre = "Juan", estado = "Disponible" },
                new { nombre = "Pedro", estado = "Ocupado" },
                new { nombre = "Carlos", estado = "Disponible" }
            };

            return View();
        }
    }
}