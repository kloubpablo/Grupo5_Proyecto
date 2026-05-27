using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class CitasController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public CitasController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // 🔥 ADMIN O BARBERO
        private bool EsAdminOBarbero()
        {
            var rol = HttpContext.Session.GetString("Rol");

            return rol == "Administrador"
                || rol == "Barbero";
        }

        // 🔥 CLIENTE
        private bool EsCliente()
        {
            return HttpContext.Session
                .GetString("Rol") == "Cliente";
        }

        // 🔥 LISTA GENERAL DE CITAS
        public IActionResult Index()
        {
            if (!EsAdminOBarbero())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var citas = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .ToList();

            return View(citas);
        }

        // 🔥 MIS CITAS
        public IActionResult MisCitas()
        {
            if (!EsCliente())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var usuario = HttpContext.Session
                .GetString("NombreUsuario");

            var citas = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .Where(c =>
                    c.IdClienteNavigation.Nombre == usuario)
                .ToList();

            return View(citas);
        }

        // 🔥 CREAR CITA (GET)
        public IActionResult Crear()
        {
            if (!EsAdminOBarbero() && !EsCliente())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // 🔥 LISTA DE BARBEROS
            ViewBag.Barberos = _context.Usuarios
                .Where(u => u.IdRol == 2)
                .ToList();

            return View();
        }

        // 🔥 CREAR CITA (POST)
        [HttpPost]
        public IActionResult Crear(
            DateOnly fecha,
            TimeOnly hora,
            string barbero)
        {
            if (!EsAdminOBarbero() && !EsCliente())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // 🔥 VALIDAR CAMPOS
            if (string.IsNullOrEmpty(barbero))
            {
                ViewBag.Error =
                    "Debe completar todos los campos";

                ViewBag.Barberos = _context.Usuarios
                    .Where(u => u.IdRol == 2)
                    .ToList();

                return View();
            }

            // 🔥 VALIDAR HORARIO
            bool ocupado = _context.Citas.Any(c =>
                c.Fecha == fecha &&
                c.Hora == hora &&
                c.Barbero == barbero);

            if (ocupado)
            {
                ViewBag.Error =
                    "Horario no disponible";

                ViewBag.Barberos = _context.Usuarios
                    .Where(u => u.IdRol == 2)
                    .ToList();

                return View();
            }

            // 🔥 CLIENTE DESDE SESSION
            var nombreCliente = HttpContext.Session
                .GetString("NombreUsuario");

            // 🔍 BUSCAR CLIENTE
            var clienteExistente = _context.Clientes
                .FirstOrDefault(c =>
                    c.Nombre == nombreCliente);

            // 🔥 CREAR CLIENTE SI NO EXISTE
            if (clienteExistente == null)
            {
                clienteExistente = new Cliente
                {
                    Nombre = nombreCliente,
                    Telefono = "00000000",
                    FechaRegistro = DateTime.Now
                };

                _context.Clientes.Add(clienteExistente);

                _context.SaveChanges();
            }

            // 🔥 CREAR CITA
            var nuevaCita = new Cita
            {
                IdCliente = clienteExistente.IdCliente,
                Fecha = fecha,
                Hora = hora,
                Barbero = barbero,
                Estado = "Activa"
            };

            _context.Citas.Add(nuevaCita);

            _context.SaveChanges();

            // 🔥 REDIRECCIONES
            if (EsCliente())
            {
                return RedirectToAction("MisCitas");
            }

            return RedirectToAction("Index");
        }

        // 🔥 CANCELAR CITA
        public IActionResult Cancelar(int id)
        {
            if (!EsAdminOBarbero() && !EsCliente())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var cita = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .FirstOrDefault(c =>
                    c.IdCita == id);

            if (cita != null)
            {
                // 🔥 CLIENTE SOLO CANCELA SUS CITAS
                if (EsCliente())
                {
                    var usuario = HttpContext.Session
                        .GetString("NombreUsuario");

                    if (cita.IdClienteNavigation.Nombre != usuario)
                    {
                        return RedirectToAction("MisCitas");
                    }
                }

                cita.Estado = "Cancelada";

                _context.SaveChanges();
            }

            // 🔥 REDIRECCIONES
            if (EsCliente())
            {
                return RedirectToAction("MisCitas");
            }

            return RedirectToAction("Index");
        }
    }
}