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
        private readonly YampiBarbershopContext _context;

        public YampiBarberShopController(
            ILogger<HomeController> logger,
            IWebHostEnvironment env,
            YampiBarbershopContext context)
        {
            _logger = logger;
            _env = env;
            _context = context;
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
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult Galeria()
        {
            ViewBag.Total = Directory.GetFiles(
                Path.Combine(
                    _env.WebRootPath,
                    "imagenes",
                    "galeria")
                ).Length;

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
            var contacto = _context.Contactos.FirstOrDefault();
            return View(contacto);
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

        public IActionResult ControlContacto()
        {
            var contacto = _context.Contactos.FirstOrDefault();

            return View(contacto);
        }

        public IActionResult ControlServicio()
        {
            return View();
        }

        // ============================
        // EDITAR CONTACTO
        // ============================

        [HttpGet]
        public IActionResult EditarContacto(int id)
        {


            var contacto = _context.Contactos.FirstOrDefault(c => c.Id == id);

            if (contacto == null)
            {
                return RedirectToAction(nameof(ControlContacto));
            }

            return View(contacto);
        }

        [HttpPost]
        public IActionResult EditarContacto(
            int id,
            string direccion,
            string telefonos,
            string correo,
            string horarios,
            string whatsapp,
            string instagram,
            string facebook,
            string tiktok,
            string ubicacion)
        {


            var contacto = _context.Contactos.FirstOrDefault(c => c.Id == id);

            if (contacto != null)
            {
                contacto.Direccion = direccion;
                contacto.Telefonos = telefonos;
                contacto.Correo = correo;
                contacto.Horarios = horarios;
                contacto.Whatsapp = whatsapp;
                contacto.Instagram = instagram;
                contacto.Facebook = facebook;
                contacto.Tiktok = tiktok;
                contacto.Ubicacion = ubicacion;

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ControlContacto));
        }

        // ============================
        // AGREGAR IMAGEN
        // ============================

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

                using (var stream = new FileStream(
                    rutaCompleta,
                    FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }
            }

            return RedirectToAction(nameof(ControlGaleria));
        }

        // ============================
        // ELIMINAR IMAGEN
        // ============================

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