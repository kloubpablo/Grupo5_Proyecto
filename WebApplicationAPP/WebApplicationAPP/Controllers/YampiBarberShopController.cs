using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using System.IO;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class YampiBarberShopController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public YampiBarberShopController(ILogger<HomeController> logger, IWebHostEnvironment env)       
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Galeria()
        {
            ViewBag.Total = Directory.GetFiles(
                Path.Combine(_env.WebRootPath,
                "imagenes",
                "galeria")).Length;
            return View();
        }
        public IActionResult Nosotros()
        {
            return View();
        }
        public IActionResult Servicios()
        {
            return View();
        }
        public IActionResult Contactenos()
        {
            return View();
        }
    }
}
