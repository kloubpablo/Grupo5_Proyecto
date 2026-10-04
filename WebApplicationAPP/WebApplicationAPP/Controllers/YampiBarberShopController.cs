using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ============================
        // Yampi BarberShop
        // ============================

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

        // ============================
        // Galeria
        // ============================

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
        // ============================
        // Nosotros
        // ============================

        public IActionResult Nosotros()
        {
            return View();
        }

        // ============================
        // Servicio
        // ============================

        public IActionResult Servicios()
        {
            var servicios = _context.Servicios.ToList();
            return View(servicios);
        }
        // ============================
        // Eliminar Servicio
        // ============================

        public IActionResult EliminarServicio(int id)
        {
            var servicio = _context.Servicios
                .FirstOrDefault(s => s.Id == id);

            if (servicio != null)
            {
                _context.Servicios.Remove(servicio);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ControlServicio));
        }

        // ============================
        // Editar Servicio
        // ============================

        [HttpGet]
        public IActionResult EditarServicio(int id)
        {
            var servicio = _context.Servicios
                .FirstOrDefault(s => s.Id == id);

            if (servicio == null)
            {
                return RedirectToAction(nameof(ControlServicio));
            }

            return View(servicio);
        }

        [HttpPost]
        public IActionResult EditarServicio(
            int id,
            string nombre,
            decimal precio,
            string descripcion)
        {
            var servicio = _context.Servicios
                .FirstOrDefault(s => s.Id == id);

            if (servicio != null)
            {
                servicio.Nombre = nombre;
                servicio.Precio = precio;
                servicio.Descripcion = descripcion;

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(ControlServicio));
        }
        // ============================
        // Agregar Servicio
        // ============================
        [HttpGet]
        public IActionResult AgregarServicio()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AgregarServicio(
            string nombre,
            decimal precio,
            string descripcion)
        {
            if (string.IsNullOrEmpty(nombre) ||
                precio <= 0 ||
                string.IsNullOrEmpty(descripcion))
            {
                ViewBag.Error = "Todos los campos son obligatorios y el precio debe ser mayor a cero.";
                return View();
            }

            Servicio servicio = new Servicio();

            servicio.Nombre = nombre;
            servicio.Precio = precio;
            servicio.Descripcion = descripcion;

            _context.Servicios.Add(servicio);
            _context.SaveChanges();

            return RedirectToAction(nameof(ControlServicio));
        }
        // ============================
        // Control Servicio
        // ============================

        public IActionResult ControlServicio()
        {
            var servicios = _context.Servicios.ToList();
            return View(servicios);
        }

        // ============================
        // Contactenos
        // ============================

        public IActionResult Contactenos()
        {
            var contacto = _context.Contactos.FirstOrDefault();
            return View(contacto);
        }

        // ============================
        // Control Contacto
        // ============================

        public IActionResult ControlContacto()
        {
            var contacto = _context.Contactos.FirstOrDefault();

            return View(contacto);
        }

        // ============================
        // Editar Contacto
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


        // ============================
        // Control Galeria
        // ============================
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
        // Agregar imagen
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
        // Eliminar Imagen
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