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
        public IActionResult ControlGaleria()
        {
            string ruta = Path.Combine(
                _env.WebRootPath,
                "imagenes",
                "galeria");

            var imagenes = Directory
                .GetFiles(ruta)
                .Select(Path.GetFileName)
                .ToList();

            return View(imagenes);
        }

        [HttpGet]
        public IActionResult AgregarImagen()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AgregarImagen(IFormFile archivo)
        {
            if (archivo != null)
            {
                string carpeta = Path.Combine(
                    _env.WebRootPath,
                    "imagenes",
                    "galeria");

                var numeros = Directory.GetFiles(carpeta)
                    .Select(x => Path.GetFileNameWithoutExtension(x))
                    .Where(x => int.TryParse(x, out _))
                    .Select(int.Parse)
                    .ToList();

                int siguienteNumero = numeros.Any()
                    ? numeros.Max() + 1
                    : 1;

                string extension = Path.GetExtension(archivo.FileName);

                string nombreArchivo = $"{siguienteNumero}{extension}";

                string rutaCompleta = Path.Combine(
                    carpeta,
                    nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }
            }

            return RedirectToAction(nameof(ControlGaleria));
        }

        public IActionResult EliminarImagen(string nombre)
        {
            string ruta = Path.Combine(
                _env.WebRootPath,
                "imagenes",
                "galeria",
                nombre);

            if (System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }

            return RedirectToAction(nameof(ControlGaleria));
        }
    }
}
