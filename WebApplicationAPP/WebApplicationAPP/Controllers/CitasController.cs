using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;

namespace WebApplicationAPP.Controllers
{
    public class CitasController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public CitasController(YampiBarbershopContext context)
        {
            _context = context;
        }

        //PERMISOS CENTRALIZADO
        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";

            return PermisosHelper.TienePermiso(_context, rol, permiso);
        }

        //LISTA GENERAL DE CITAS
        public IActionResult Index()
        {
            if (!TienePermiso("Citas/Index"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var citas = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .ToList();

            return View(citas);
        }

        //MIS CITAS
        public IActionResult MisCitas()
        {
            if (!TienePermiso("Citas/MisCitas"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var usuario = HttpContext.Session.GetString("NombreUsuario") ?? "";

            var citas = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .Where(c => c.IdClienteNavigation.Nombre == usuario)
                .ToList();

            return View(citas);
        }


        //HISTORIAL DE CITAS
        public IActionResult Historial()
        {
            if (!TienePermiso("Citas/MisCitas"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var usuario = HttpContext.Session.GetString("NombreUsuario") ?? "";

            var historial = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .Where(c => c.IdClienteNavigation.Nombre == usuario)
                .OrderByDescending(c => c.Fecha)
                .ThenByDescending(c => c.Hora)
                .ToList();

            return View(historial);
        }

        //CREAR CITA (GET)
        public IActionResult Crear()
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Barberos = _context.Usuarios
                .Where(u => u.IdRol == 2)
                .ToList();

            return View();
        }

        // CREAR CITA (POST)
        [HttpPost]
        public IActionResult Crear(DateOnly fecha, TimeOnly hora, string barbero)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.IsNullOrWhiteSpace(barbero))
            {
                ViewBag.Error = "Debe completar todos los campos";

                ViewBag.Barberos = _context.Usuarios
                    .Where(u => u.IdRol == 2)
                    .ToList();

                return View();
            }

            bool ocupado = _context.Citas.Any(c =>
                c.Fecha == fecha &&
                c.Hora == hora &&
                c.Barbero == barbero);

            if (ocupado)
            {
                ViewBag.Error = "Horario no disponible";

                ViewBag.Barberos = _context.Usuarios
                    .Where(u => u.IdRol == 2)
                    .ToList();

                return View();
            }

            var nombreCliente = HttpContext.Session.GetString("NombreUsuario") ?? "";

            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                return RedirectToAction("Index", "Seguridad");
            }

            var clienteExistente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == nombreCliente);

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

            return RedirectToAction("MisCitas");
        }

        // 🔥 CANCELAR CITA
        public IActionResult Cancelar(int id)
        {
            if (!TienePermiso("Citas/Index"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var cita = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .FirstOrDefault(c => c.IdCita == id);

            if (cita != null)
            {
                var usuario = HttpContext.Session.GetString("NombreUsuario") ?? "";

                if (TienePermiso("Citas/MisCitas") &&
                    !TienePermiso("Citas/Index"))
                {
                    if (cita.IdClienteNavigation.Nombre != usuario)
                    {
                        return RedirectToAction("MisCitas");
                    }
                }

                cita.Estado = "Cancelada";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}