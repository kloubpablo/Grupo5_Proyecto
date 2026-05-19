using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class HomeController : Controller
    {
        // Página principal
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // Privacidad
        public IActionResult Privacy()
        {
            return View();
        }

        // Errores
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = HttpContext.TraceIdentifier
            });
        }
    }
}