using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using WebApplicationAPP.Models;
using System.Net;
using System.Text.RegularExpressions;
using System.Globalization;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarContacto(
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
            var contacto = _context.Contactos
                .FirstOrDefault(c => c.Id == id);

            if (contacto == null)
            {
                TempData["Error"] = "No se encontro el contacto.";
                return RedirectToAction(nameof(ControlContacto));
            }

            string enlace = (ubicacion ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(enlace))
            {
                string? mapaEmbed =
                    await ConvertirGoogleMapsAsync(enlace);

                if (string.IsNullOrWhiteSpace(mapaEmbed))
                {
                    TempData["Error"] =
                        "No se pudo obtener la ubicacion del enlace. " +
                        "Comprueba que sea un enlace valido de Google Maps.";

                    return RedirectToAction(nameof(ControlContacto));
                }

                enlace = mapaEmbed;
            }

            contacto.Direccion = direccion;
            contacto.Telefonos = telefonos;
            contacto.Correo = correo;
            contacto.Horarios = horarios;
            contacto.Whatsapp = whatsapp;
            contacto.Instagram = instagram;
            contacto.Facebook = facebook;
            contacto.Tiktok = tiktok;
            contacto.Ubicacion = enlace;

            _context.SaveChanges();

            TempData["Mensaje"] =
                "Informacion de contacto actualizada correctamente.";

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
        private static bool EsDominioGoogleMaps(Uri url)
        {
            if (url.Scheme != Uri.UriSchemeHttps)
                return false;

            string host = url.Host.ToLowerInvariant();

            return host == "maps.app.goo.gl"
                || host == "goo.gl"
                || host == "google.com"
                || host.EndsWith(".google.com")
                || host == "google.co.cr"
                || host.EndsWith(".google.co.cr");
        }

        private static string? ObtenerCoordenadas(string enlace)
        {

            var coordenadasLugar = Regex.Match(
                enlace,
                @"!3d(-?\d+(?:\.\d+)?)!4d(-?\d+(?:\.\d+)?)"
            );

            if (coordenadasLugar.Success)
            {
                return coordenadasLugar.Groups[1].Value
                    + ","
                    + coordenadasLugar.Groups[2].Value;
            }

            var coordenadasMapa = Regex.Match(
                enlace,
                @"@(-?\d+(?:\.\d+)?),(-?\d+(?:\.\d+)?)"
            );

            if (coordenadasMapa.Success)
            {
                return coordenadasMapa.Groups[1].Value
                    + ","
                    + coordenadasMapa.Groups[2].Value;
            }

            return null;
        }

        private async Task<string?> ConvertirGoogleMapsAsync(
            string enlace)
        {
            if (!Uri.TryCreate(
                enlace,
                UriKind.Absolute,
                out Uri? url))
            {
                return null;
            }

            if (!EsDominioGoogleMaps(url))
                return null;

            if (
                (url.Host == "www.google.com" ||
                url.Host == "google.com") &&
                url.AbsolutePath.StartsWith(
                    "/maps/embed",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return enlace;
            }

            string urlFinal = enlace;

            if (
                url.Host == "maps.app.goo.gl" ||
                url.Host == "goo.gl"
            )
            {
                using var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false
                };

                using var cliente = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                Uri actual = url;

                for (int intento = 0; intento < 8; intento++)
                {
                    if (!EsDominioGoogleMaps(actual))
                        return null;

                    using var respuesta = await cliente.GetAsync(
                        actual,
                        HttpCompletionOption.ResponseHeadersRead
                    );

                    int estado = (int)respuesta.StatusCode;

                    if (
                        estado >= 300 &&
                        estado < 400 &&
                        respuesta.Headers.Location != null
                    )
                    {
                        actual = new Uri(
                            actual,
                            respuesta.Headers.Location
                        );

                        continue;
                    }

                    if (!respuesta.IsSuccessStatusCode)
                        return null;

                    break;
                }

                urlFinal = actual.ToString();
            }

            string? coordenadas =
                ObtenerCoordenadas(urlFinal);

            if (string.IsNullOrWhiteSpace(coordenadas))
                return null;

            return "https://www.google.com/maps?q="
                + Uri.EscapeDataString(coordenadas)
                + "&output=embed";
        }


    }
}