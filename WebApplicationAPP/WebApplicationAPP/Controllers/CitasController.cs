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
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
    DateOnly fecha,
    TimeOnly hora,
    string barbero)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            // Validar campos obligatorios
            if (fecha == default ||
                hora == default ||
                string.IsNullOrWhiteSpace(barbero))
            {
                TempData["ErrorCita"] =
                    "Debe completar todos los campos.";
                TempData["SeccionCita"] = "crear";
                return RedirectToAction("Index");
            }
            barbero = barbero.Trim();
            // Validar hora de Costa Rica
            if (HorarioInvalido(fecha, hora))
            {
                TempData["ErrorCita"] =
                    "Solo se permiten citas de lunes a sabado, de 7:00 AM a 8:30 PM, " +
                    "en intervalos de 30 minutos y horas futuras de Costa Rica.";
                TempData["SeccionCita"] = "crear";
                return RedirectToAction("Index");
            }
            // Verificar que el barbero este activo
            bool barberoActivo = _context.Usuarios.Any(u =>
                u.IdRol == 2 &&
                u.Estado &&
                u.Nombre == barbero
            );
            if (!barberoActivo)
            {
                TempData["ErrorCita"] =
                    "El barbero seleccionado no esta disponible.";
                TempData["SeccionCita"] = "crear";
                return RedirectToAction("Index");
            }
            // Mantener la comprobacion y el guardado en la misma transaccion.
            using var transaccion = _context.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable);
            // Evitar horarios duplicados o bloqueados
            if (HorarioOcupado(barbero, fecha, hora))
            {
                TempData["ErrorCita"] =
                    "Este barbero ya tiene una cita o bloqueo " +
                    "en la fecha y hora seleccionadas.";
                TempData["SeccionCita"] = "crear";
                return RedirectToAction("Index");
            }
            // Obtener cliente desde la sesion
            var nombreCliente =
                HttpContext.Session.GetString("NombreUsuario") ?? "";
            if (string.IsNullOrWhiteSpace(nombreCliente))
            {
                return RedirectToAction("Index", "Seguridad");
            }
            var clienteExistente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == nombreCliente);
            // Crear cliente si no existe
            if (clienteExistente == null)
            {
                clienteExistente = new Cliente
                {
                    Nombre = nombreCliente,
                    Telefono = "00000000",
                    FechaRegistro = AhoraCostaRica()
                };
                _context.Clientes.Add(clienteExistente);
                _context.SaveChanges();
            }
            // Crear cita
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
            transaccion.Commit();
            TempData["MensajeCita"] =
                "Cita creada correctamente.";
            TempData["SeccionCita"] = "agenda";
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Cita cita)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            // Buscar cita original
            var citaDB = _context.Citas
                .FirstOrDefault(c => c.IdCita == cita.IdCita);
            if (citaDB == null)
            {
                TempData["ErrorCita"] =
                    "No se encontro la cita seleccionada.";
                return RedirectToAction("Index");
            }
            // Validar campos
            if (cita.Fecha == default ||
                cita.Hora == default ||
                string.IsNullOrWhiteSpace(cita.Barbero))
            {
                TempData["ErrorCita"] =
                    "Debe completar todos los campos.";
                TempData["SeccionCita"] = "editar";
                TempData["EditarId"] = cita.IdCita;
                return RedirectToAction("Index");
            }
            string barbero = cita.Barbero.Trim();
            // Validar hora de Costa Rica
            if (HorarioInvalido(cita.Fecha, cita.Hora))
            {
                TempData["ErrorCita"] =
                    "Solo se permiten citas de lunes a sabado, de 7:00 AM a 8:30 PM, " +
                    "en intervalos de 30 minutos y horas futuras de Costa Rica.";
                TempData["SeccionCita"] = "editar";
                TempData["EditarId"] = cita.IdCita;
                return RedirectToAction("Index");
            }
            // Verificar que el barbero este activo
            bool barberoActivo = _context.Usuarios.Any(u =>
                u.IdRol == 2 &&
                u.Estado &&
                u.Nombre == barbero
            );
            if (!barberoActivo)
            {
                TempData["ErrorCita"] =
                    "El barbero seleccionado no esta disponible.";
                TempData["SeccionCita"] = "editar";
                TempData["EditarId"] = cita.IdCita;
                return RedirectToAction("Index");
            }
            // Mantener la comprobacion y el guardado en la misma transaccion.
            using var transaccion = _context.Database.BeginTransaction(
                System.Data.IsolationLevel.Serializable);
            // Validar conflicto, excluyendo esta misma cita
            if (HorarioOcupado(
                barbero,
                cita.Fecha,
                cita.Hora,
                cita.IdCita))
            {
                TempData["ErrorCita"] =
                    "Este barbero ya tiene una cita o bloqueo " +
                    "en ese horario.";
                TempData["SeccionCita"] = "editar";
                TempData["EditarId"] = cita.IdCita;
                return RedirectToAction("Index");
            }
            // Actualizar solamente los campos editables
            citaDB.Fecha = cita.Fecha;
            citaDB.Hora = cita.Hora;
            citaDB.Barbero = barbero;
            _context.SaveChanges();
            transaccion.Commit();
            TempData["MensajeCita"] =
                "Cita actualizada correctamente.";
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
        private static DateTime AhoraCostaRica()
        {
            // Obtener la hora real de Costa Rica
            TimeZoneInfo zona;
            try
            {
                zona = TimeZoneInfo.FindSystemTimeZoneById(
                    "America/Costa_Rica"
                );
            }
            catch (TimeZoneNotFoundException)
            {
                zona = TimeZoneInfo.FindSystemTimeZoneById(
                    "Central America Standard Time"
                );
            }
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                zona
            );
        }
        // HORARIO DE ATENCION Y DURACION FIJA DE 30 MINUTOS
        private static bool HorarioInvalido(DateOnly fecha, TimeOnly hora)
        {
            var ahora = AhoraCostaRica();
            var hoy = DateOnly.FromDateTime(ahora);
            // Los domingos la barberia permanece cerrada.
            if (fecha == default || fecha.DayOfWeek == DayOfWeek.Sunday)
                return true;
            // Solo se permiten inicios cada media hora.
            if (hora.Minute != 0 && hora.Minute != 30)
                return true;
            // Desde las 7:00 hasta las 20:30 inclusive.
            // La ultima cita termina a las 21:00.
            if (hora < new TimeOnly(7, 0) || hora > new TimeOnly(20, 30))
                return true;
            if (fecha < hoy)
                return true;
            // Bloquear la hora actual completa, como solicita el negocio.
            // Por ejemplo: 14:09 bloquea 14:00 y 14:30.
            if (fecha == hoy && hora.Hour <= ahora.Hour)
                return true;
            return false;
        }
        // DETECTAR INTERVALOS QUE SE SUPERPONEN
        private bool HorarioOcupado(
            string barbero,
            DateOnly fecha,
            TimeOnly hora,
            int? excluirId = null)
        {
            string nombre = barbero.Trim();
            // Consultar solo las citas relevantes del mismo dia y barbero.
            var horasRegistradas = _context.Citas
                .Where(c => c.Fecha == fecha &&
                    c.Barbero != null &&
                    c.Barbero.Trim() == nombre &&
                    (c.Estado == "Activa" || c.Estado == "Bloqueada") &&
                    (!excluirId.HasValue || c.IdCita != excluirId.Value))
                .Select(c => c.Hora)
                .ToList();
            int nuevoInicio = hora.Hour * 60 + hora.Minute;
            int nuevoFin = nuevoInicio + 30;
            // Se considera que una cita dura 30 minutos.
            // Tambien protege contra citas antiguas iniciadas a :15 o :45.
            return horasRegistradas.Any(h =>
            {
                int inicio = h.Hour * 60 + h.Minute;
                int fin = inicio + 30;
                return inicio < nuevoFin && nuevoInicio < fin;
            });
        }
        // ==========================================
        // CONSULTAR HORARIOS OCUPADOS
        // ==========================================
        [HttpGet]
        public IActionResult HorariosOcupados(
            string barbero,
            DateOnly fecha,
            int? excluirId = null)
        {
            if (!TienePermiso("Citas/Crear"))
            {
                return Forbid();
            }
            if (string.IsNullOrWhiteSpace(barbero) ||
                fecha == default)
            {
                return BadRequest(new
                {
                    error = "Debe seleccionar barbero y fecha."
                });
            }
            string nombre = barbero.Trim();
            var horas = _context.Citas
                .AsNoTracking()
                .Where(c =>
                    c.Fecha == fecha &&
                    c.Barbero != null &&
                    c.Barbero.Trim() == nombre &&
                    (
                        c.Estado == "Activa" ||
                        c.Estado == "Bloqueada"
                    ) &&
                    (
                        !excluirId.HasValue ||
                        c.IdCita != excluirId.Value
                    )
                )
                .Select(c => c.Hora)
                .ToList();
            return Json(new
            {
                horasOcupadas = horas
                    .Select(h => h.ToString("HH:mm"))
                    .ToList()
            });
        }
    }
}
