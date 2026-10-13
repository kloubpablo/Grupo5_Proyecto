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

        // PERMISOS CENTRALIZADO
        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";

            return PermisosHelper.TienePermiso(_context, rol, permiso);
        }

        // LISTA GENERAL DE CITAS
        public IActionResult Index(int? editarId, DateOnly? fechaHorarios)
        {
            if (!TienePermiso("Citas/Index"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var citas = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .ToList();

            // EDITAR
            if (editarId.HasValue)
            {
                var citaEditar = _context.Citas
                    .FirstOrDefault(c => c.IdCita == editarId.Value);

                if (citaEditar != null)
                {
                    ViewBag.CitaEditar = citaEditar;

                    ViewBag.Barberos = _context.Usuarios
                        .Where(u => u.IdRol == 2)
                        .ToList();
                }
            }

            // HORARIOS
            if (fechaHorarios.HasValue)
            {
                ViewBag.FechaHorarios = fechaHorarios.Value;

                ViewBag.Horarios = _context.Citas
                    .Where(c => c.Fecha == fechaHorarios.Value)
                    .OrderBy(c => c.Hora)
                    .ToList();
            }

            // BARBEROS PARA CREAR
            ViewBag.Barberos = _context.Usuarios
                .Where(u => u.IdRol == 2)
                .ToList();

            return View(citas);
        }

        // MIS CITAS
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
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .ToList();

            return View(citas);
        }

        public IActionResult test()
        {
            return View();
        }

        // CREAR CITA
        [HttpPost]
        public IActionResult Crear(DateOnly fecha, TimeOnly hora, string barbero)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.IsNullOrWhiteSpace(barbero))
            {
                TempData["ErrorCita"] = "Debe completar todos los campos";
                TempData["SeccionCita"] = "crear";

                return RedirectToAction("Index");
            }

            bool ocupado = _context.Citas.Any(c =>
                c.Fecha == fecha &&
                c.Hora == hora &&
                c.Barbero == barbero &&
                (c.Estado == "Activa" || c.Estado == "Bloqueada"));

            if (ocupado)
            {
                TempData["ErrorCita"] = "Horario no disponible";
                TempData["SeccionCita"] = "crear";

                return RedirectToAction("Index");
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

            TempData["MensajeCita"] = "Cita creada correctamente";
            TempData["SeccionCita"] = "agenda";

            return RedirectToAction("Index");
        }

        // EDITAR CITA
        [HttpPost]
        public IActionResult Editar(Cita cita)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (string.IsNullOrWhiteSpace(cita.Barbero))
            {
                TempData["ErrorCita"] = "Debe completar todos los campos";
                TempData["SeccionCita"] = "editar";
                TempData["EditarId"] = cita.IdCita;

                return RedirectToAction("Index");
            }

            bool ocupado = _context.Citas.Any(c =>
                c.IdCita != cita.IdCita &&
                c.Fecha == cita.Fecha &&
                c.Hora == cita.Hora &&
                c.Barbero == cita.Barbero &&
                (c.Estado == "Activa" || c.Estado == "Bloqueada"));

            if (ocupado)
            {
                TempData["ErrorCita"] = "Ese horario ya está ocupado.";
                TempData["SeccionCita"] = "editar";
                TempData["EditarId"] = cita.IdCita;

                return RedirectToAction("Index");
            }

            var citaDB = _context.Citas
                .FirstOrDefault(c => c.IdCita == cita.IdCita);

            if (citaDB == null)
            {
                return RedirectToAction("Index");
            }

            citaDB.Fecha = cita.Fecha;
            citaDB.Hora = cita.Hora;
            citaDB.Barbero = cita.Barbero;

            _context.SaveChanges();

            TempData["MensajeCita"] = "Cita actualizada correctamente";
            TempData["SeccionCita"] = "agenda";

            return RedirectToAction("Index");
        }

        // CANCELAR CITA
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

        // BLOQUEAR HORARIO
        public IActionResult Bloquear(int id)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var cita = _context.Citas
                .FirstOrDefault(c => c.IdCita == id);

            if (cita != null)
            {
                cita.Estado = "Bloqueada";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // HORARIOS DISPONIBLES
        [HttpPost]
        public IActionResult HorariosDisponibles(DateOnly fecha)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return RedirectToAction(
                "Index",
                new { fechaHorarios = fecha }
            );
        }
    }
}