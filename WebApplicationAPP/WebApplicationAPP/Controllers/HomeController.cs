using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class HomeController : Controller
    {
        // Página principal (puedes redirigir al dashboard)
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Vista de privacidad (opcional)
        public IActionResult Privacy()
        {
            return View();
        }

        // Manejo de errores
        public IActionResult Error()
        {
            var model = new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier
            };

            return View(model);
        }
    }
}