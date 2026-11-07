using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;
using WebApplicationAPP.Services;

namespace WebApplicationAPP.Controllers
{
    // La pagina unificada sirve a Administrador y Cliente. Cada consulta queda
    // acotada en el servidor, incluso cuando se llama directamente a una URL.
    public class CitasController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public CitasController(YampiBarbershopContext context) => _context = context;

        private Usuario? UsuarioActual()
        {
            int? id = HttpContext.Session.GetInt32("IdUsuario");
            if (!id.HasValue) return null;
            return _context.Usuarios.AsNoTracking()
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u => u.IdUsuario == id.Value && u.Estado && !u.ContraTemp && u.IdRolNavigation.Estado);
        }

        private HashSet<string>? _permisosActuales;

        private bool Permiso(Usuario usuario, string clave)
        {
            if (EsAdmin(usuario)) return true;
            // Una sola politica centralizada para Roles y todas las acciones.
            _permisosActuales ??= PermisosHelper.ObtenerPermisos(_context, usuario);
            return _permisosActuales.Contains(clave);
        }

        private static bool EsAdmin(Usuario usuario) =>
            usuario.IdRolNavigation.Nombre == "Administrador";

        private static bool EsCliente(Usuario usuario) =>
            usuario.IdRolNavigation.Nombre == "Cliente";

        private static bool EsBarbero(Usuario usuario) =>
            usuario.IdRolNavigation.Nombre == "Barbero";

        private IActionResult SinAcceso(Usuario? usuario) =>
            usuario == null ? RedirectToAction("Index", "Seguridad") : StatusCode(403);

        // No usar nombres: id_usuario es la unica identidad del propietario.
        private int? ClienteActualId(Usuario usuario) => _context.Clientes
            .Where(c => c.IdUsuario == usuario.IdUsuario)
            .Select(c => (int?)c.IdCliente)
            .FirstOrDefault();

        private bool PuedeVerPortal(Usuario u) => EsAdmin(u) ||
            ((EsCliente(u) || EsBarbero(u)) &&
             (Permiso(u, "Citas/MisCitas") ||
              Permiso(u, "Citas/Crear") ||
              Permiso(u, "Citas/HorariosDisponibles")));

        private bool PuedeCrear(Usuario u) => EsAdmin(u) ||
            ((EsCliente(u) || EsBarbero(u)) && Permiso(u, "Citas/Crear"));

        private bool PuedeHorarios(Usuario u) => EsAdmin(u) ||
            ((EsCliente(u) || EsBarbero(u)) &&
             (Permiso(u, "Citas/HorariosDisponibles") || Permiso(u, "Citas/Crear")));

        private bool PuedeMisCitas(Usuario u) => EsAdmin(u) ||
            ((EsCliente(u) || EsBarbero(u)) && Permiso(u, "Citas/MisCitas"));

        private bool PuedeEditar(Usuario u) => EsAdmin(u) ||
            ((EsCliente(u) || EsBarbero(u)) && Permiso(u, "Citas/Editar"));

        private bool PuedeCancelar(Usuario u) => EsAdmin(u) ||
            ((EsCliente(u) || EsBarbero(u)) && Permiso(u, "Citas/Cancelar"));

        // UNICA PAGINA: administrador todas; cliente solo las suyas.
        [HttpGet]
        public IActionResult Index(int? editarId, DateOnly? fechaHorarios)
        {
            var usuario = UsuarioActual();
            if (usuario == null || !PuedeVerPortal(usuario)) return SinAcceso(usuario);

            bool admin = EsAdmin(usuario);
            bool barbero = EsBarbero(usuario);
            int? clienteId = EsCliente(usuario) ? ClienteActualId(usuario) : null;
            var consulta = _context.Citas.AsNoTracking()
                .Include(c => c.IdClienteNavigation).AsQueryable();

            if (barbero)
            {
                // El barbero nunca puede leer la agenda de otro barbero.
                consulta = consulta.Where(c => c.IdBarbero == usuario.IdUsuario);
            }
            else if (!admin)
            {
                // Un cliente solo consulta sus propias citas vinculadas por ID.
                consulta = clienteId.HasValue
                    ? consulta.Where(c => c.IdCliente == clienteId.Value)
                    : consulta.Where(c => false);
            }

            var citas = PuedeMisCitas(usuario)
                ? consulta.OrderBy(c => c.Fecha).ThenBy(c => c.Hora).ToList()
                : new List<Cita>();


            // Obtener los ID de las citas visibles
            var idsCitas = citas.Select(c => c.IdCita).ToList();

            // Identificar citas que ya fueron recibidas

            var citasRecibidas = _context.Atencions
                .AsNoTracking()
                .Where(a => a.IdCita.HasValue &&
                            idsCitas.Contains(a.IdCita.GetValueOrDefault()))
                .Select(a => a.IdCita.GetValueOrDefault())
                .ToHashSet();


            // Enviar la informacion a la vista
            ViewBag.CitasRecibidas = citasRecibidas;


            ViewBag.EsAdministrador = admin;
            ViewBag.EsBarbero = barbero;
            ViewBag.EsCliente = EsCliente(usuario);
            ViewBag.PuedeCrear = PuedeCrear(usuario);
            ViewBag.PuedeHorarios = PuedeHorarios(usuario);
            ViewBag.PuedeMisCitas = PuedeMisCitas(usuario);
            ViewBag.PuedeEditar = PuedeEditar(usuario);
            ViewBag.PuedeCancelar = PuedeCancelar(usuario);
            ViewBag.PuedeRecibir = EsAdmin(usuario) ||
                (EsBarbero(usuario) && Permiso(usuario, "Atencion/RegistrarDesdeCita"));
            ViewBag.SinVinculacion = EsCliente(usuario) && !clienteId.HasValue;

            // Solo el administrador y los clientes pueden escoger cualquier barbero.
            // El barbero autorizado para crear reservas solo se puede elegir a si mismo.
            var barberosConsulta = _context.Usuarios.AsNoTracking()
                .Where(u => u.IdRolNavigation.Nombre == "Barbero" &&
                    u.IdRolNavigation.Estado && u.Estado && !u.ContraTemp);
            if (barbero)
                barberosConsulta = barberosConsulta.Where(u => u.IdUsuario == usuario.IdUsuario);
            ViewBag.Barberos = barberosConsulta.OrderBy(u => u.Nombre).ToList();

            if (editarId.HasValue && PuedeEditar(usuario))
            {
                var citaEditar = consulta.FirstOrDefault(c => c.IdCita == editarId.Value);

                if (citaEditar != null &&
                    citaEditar.Estado == "Activa" &&
                    !citasRecibidas.Contains(citaEditar.IdCita))
                {
                    ViewBag.CitaEditar = citaEditar;
                }

            }

            if (fechaHorarios.HasValue && PuedeHorarios(usuario))
            {
                ViewBag.FechaHorarios = fechaHorarios.Value;
                ViewBag.Disponibilidades = ObtenerDisponibilidad(
                    fechaHorarios.Value, barbero ? usuario.IdUsuario : null);
            }
            return View(citas);
        }

        // Compatibilidad de enlaces antiguos, sin una segunda vista.
        [HttpGet]
        public IActionResult MisCitas() => RedirectToAction(nameof(Index));

        [HttpGet]
        public IActionResult Crear()
        {
            var u = UsuarioActual();
            return u != null && PuedeCrear(u) ? RedirectToAction(nameof(Index)) : SinAcceso(u);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Crear(DateOnly fecha, TimeOnly hora, int idBarbero, int? idCliente)
        {
            var u = UsuarioActual();
            if (u == null || !PuedeCrear(u)) return SinAcceso(u);

            var barbero = AgendaBarberoService.BuscarBarbero(_context, idBarbero);
            if (barbero == null || !AgendaBarberoService.EsHorarioValido(fecha, hora))
            {
                TempData["ErrorCita"] = "Selecciona un barbero y un turno futuro habil (7:00 a 20:30).";
                return RedirectToAction(nameof(Index));
            }

            // Administrador y barbero autorizado seleccionan cliente existente;
            // el cliente autenticado solo puede reservar a su propio nombre.
            bool admin = EsAdmin(u);
            bool barberoAutorizado = EsBarbero(u);
            if (barberoAutorizado && idBarbero != u.IdUsuario)
                return StatusCode(403);

            if ((admin || barberoAutorizado) &&
                (!idCliente.HasValue || !_context.Clientes.Any(c => c.IdCliente == idCliente.Value)))
            {
                TempData["ErrorCita"] = "Selecciona el cliente para la reserva administrativa.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                using var tx = _context.Database.BeginTransaction(IsolationLevel.Serializable);
                if (AgendaBarberoService.Ocupado(_context, barbero.IdUsuario, barbero.Nombre, fecha, hora))
                {
                    TempData["ErrorCita"] = "Ese turno ya esta ocupado.";
                    return RedirectToAction(nameof(Index));
                }
                Cliente? cliente = null;
                if (EsCliente(u))
                {
                    cliente = _context.Clientes.FirstOrDefault(c => c.IdUsuario == u.IdUsuario);
                    if (cliente == null)
                    {
                        cliente = new Cliente
                        {
                            IdUsuario = u.IdUsuario,
                            Nombre = u.Nombre,
                            Email = u.CorreoElectronico,
                            Telefono = "00000000",
                            FechaRegistro = AgendaBarberoService.AhoraCostaRica()
                        };
                        _context.Clientes.Add(cliente);
                        _context.SaveChanges();
                    }
                }
                _context.Citas.Add(new Cita
                {
                    IdCliente = (admin || barberoAutorizado) ? idCliente!.Value : cliente!.IdCliente,
                    IdBarbero = barbero.IdUsuario,
                    Fecha = fecha,
                    Hora = hora,
                    Barbero = barbero.Nombre,
                    Estado = "Activa"
                });
                _context.SaveChanges();
                tx.Commit();
                TempData["MensajeCita"] = "Cita creada; ahora aparece en la agenda del barbero.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorCita"] = "El turno se reservo simultaneamente o los datos ya cambiaron.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Cita cita)
        {
            var u = UsuarioActual();
            if (u == null || !PuedeEditar(u)) return SinAcceso(u);
            bool admin = EsAdmin(u);
            bool barberoActual = EsBarbero(u);
            int? clienteId = (!admin && !barberoActual) ? ClienteActualId(u) : null;
            using var tx = _context.Database.BeginTransaction(IsolationLevel.Serializable);
            var original = _context.Citas.FirstOrDefault(c => c.IdCita == cita.IdCita);
            if (original == null) return NotFound();
            if (!admin && (barberoActual
                ? original.IdBarbero != u.IdUsuario
                : !clienteId.HasValue || original.IdCliente != clienteId.Value)) return StatusCode(403);
            // Un barbero no puede transferir su cita a otro barbero.
            if (barberoActual && cita.IdBarbero != u.IdUsuario) return StatusCode(403);

            if (original.Estado != "Activa")
            {
                TempData["ErrorCita"] =
                    "Solo se pueden editar citas activas.";

                return RedirectToAction(nameof(Index));
            }

            if (_context.Atencions.Any(a => a.IdCita == original.IdCita))
            {
                TempData["ErrorCita"] =
                    "No puedes editar esta cita porque el barbero ya recibio al cliente.";

                return RedirectToAction(nameof(Index));
            }


            var barbero = AgendaBarberoService.BuscarBarbero(_context, cita.IdBarbero ?? 0);
            if (barbero == null || !AgendaBarberoService.EsHorarioValido(cita.Fecha, cita.Hora))
            {
                TempData["ErrorCita"] = "La fecha, hora o barbero no es valido.";
                return RedirectToAction(nameof(Index), new { editarId = cita.IdCita });
            }
            try
            {
                if (AgendaBarberoService.Ocupado(_context, barbero.IdUsuario, barbero.Nombre,
                    cita.Fecha, cita.Hora, original.IdCita))
                {
                    TempData["ErrorCita"] = "El horario seleccionado esta ocupado.";
                    return RedirectToAction(nameof(Index), new { editarId = cita.IdCita });
                }
                // Propietario, estado y cliente no se reciben del formulario.
                original.Fecha = cita.Fecha;
                original.Hora = cita.Hora;
                original.Barbero = barbero.Nombre;
                original.IdBarbero = barbero.IdUsuario;
                _context.SaveChanges();
                tx.Commit();
                TempData["MensajeCita"] = "Cita actualizada correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorCita"] = "El turno ya no esta disponible.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(int id)
        {
            var u = UsuarioActual();
            if (u == null || !PuedeCancelar(u)) return SinAcceso(u);
            using var tx = _context.Database.BeginTransaction(IsolationLevel.Serializable);
            var cita = _context.Citas.FirstOrDefault(c => c.IdCita == id);
            if (cita == null) return NotFound();
            if (!EsAdmin(u) && (EsBarbero(u)
                ? cita.IdBarbero != u.IdUsuario
                : cita.IdCliente != ClienteActualId(u))) return StatusCode(403);

            if (cita.Estado != "Activa")
            {
                TempData["ErrorCita"] =
                    "Solo se pueden cancelar citas activas.";

                return RedirectToAction(nameof(Index));
            }

            if (_context.Atencions.Any(a => a.IdCita == id))
            {
                TempData["ErrorCita"] =
                    "No puedes cancelar esta cita porque el cliente ya fue recibido.";

                return RedirectToAction(nameof(Index));
            }

            cita.Estado = "Cancelada";
            _context.SaveChanges();
            tx.Commit();
            TempData["MensajeCita"] = "Cita cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Bloquear(int id)
        {
            var u = UsuarioActual();
            if (u == null || !EsAdmin(u)) return SinAcceso(u);
            using var tx = _context.Database.BeginTransaction(IsolationLevel.Serializable);
            var cita = _context.Citas.FirstOrDefault(c => c.IdCita == id);
            if (cita == null) return NotFound();

            if (cita.Estado != "Activa")
            {
                TempData["ErrorCita"] =
                    "Solo se pueden bloquear citas activas.";

                return RedirectToAction(nameof(Index));
            }

            if (_context.Atencions.Any(a => a.IdCita == id))
            {
                TempData["ErrorCita"] =
                    "No puedes bloquear esta cita porque ya ingreso al modulo de Atencion.";

                return RedirectToAction(nameof(Index));
            }

            cita.Estado = "Bloqueada";
            _context.SaveChanges();
            tx.Commit();
            TempData["MensajeCita"] = "Cita bloqueada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HorariosDisponibles(DateOnly fecha)
        {
            var u = UsuarioActual();
            if (u == null || !PuedeHorarios(u)) return SinAcceso(u);
            if (fecha == default) return BadRequest("Debes seleccionar una fecha.");
            return RedirectToAction(nameof(Index), new { fechaHorarios = fecha });
        }

        // Horarios por id de barbero; nunca por nombre como identidad.
        [HttpGet]
        public IActionResult HorariosOcupados(int idBarbero, DateOnly fecha, int? excluirId = null)
        {
            var u = UsuarioActual();
            if (u == null || (!PuedeCrear(u) && !PuedeEditar(u))) return SinAcceso(u);
            if (EsBarbero(u) && idBarbero != u.IdUsuario)
                return StatusCode(403);
            var barber = AgendaBarberoService.BuscarBarbero(_context, idBarbero);
            if (barber == null || fecha == default)
                return BadRequest(new { error = "Selecciona barbero y fecha." });

            if (excluirId.HasValue)
            {
                bool permitido = PuedeEditar(u) && _context.Citas.Any(c =>
                    c.IdCita == excluirId && c.Estado == "Activa" &&
                    (EsAdmin(u) || (EsBarbero(u)
                        ? c.IdBarbero == u.IdUsuario
                        : c.IdCliente == ClienteActualId(u))));
                if (!permitido) excluirId = null;
            }
            var horas = _context.Citas.AsNoTracking()
                .Where(c => c.Fecha == fecha &&
                    (c.Estado == "Activa" || c.Estado == "Bloqueada") &&
                    (c.IdBarbero == barber.IdUsuario ||
                        (c.IdBarbero == null && c.Barbero == barber.Nombre)) &&
                    (!excluirId.HasValue || c.IdCita != excluirId.Value))
                .Select(c => c.Hora).ToList();
            return Json(new { horasOcupadas = horas.Select(h => h.ToString("HH:mm")).ToList() });
        }

        private List<DisponibilidadDiaVM> ObtenerDisponibilidad(DateOnly fecha, int? soloBarberoId = null)
        {
            var barberosQuery = _context.Usuarios.AsNoTracking()
                .Where(b => b.Estado && !b.ContraTemp && b.IdRolNavigation.Estado &&
                    b.IdRolNavigation.Nombre == "Barbero");
            if (soloBarberoId.HasValue)
                barberosQuery = barberosQuery.Where(b => b.IdUsuario == soloBarberoId.Value);
            var barberos = barberosQuery
                .Select(b => new { b.IdUsuario, b.Nombre }).OrderBy(b => b.Nombre).ToList();
            var ocupadas = _context.Citas.AsNoTracking()
                .Where(c => c.Fecha == fecha && (c.Estado == "Activa" || c.Estado == "Bloqueada"))
                .Select(c => new { c.IdBarbero, c.Barbero, c.Hora }).ToList();

            return barberos.Select(b => new DisponibilidadDiaVM
            {
                IdBarbero = b.IdUsuario,
                Barbero = b.Nombre,
                HorasDisponibles = Enumerable.Range(0, 28)
                    .Select(i => new TimeOnly(7, 0).AddMinutes(i * 30))
                    .Where(h => AgendaBarberoService.EsHorarioValido(fecha, h) &&
                        !ocupadas.Any(c => c.Hora == h &&
                            (c.IdBarbero == b.IdUsuario ||
                             (c.IdBarbero == null && c.Barbero == b.Nombre))))
                    .Select(h => h.ToString("HH:mm")).ToList()
            }).ToList();
        }

        // Busqueda limitada: no se envian todos los clientes al navegador.
        // Solo administrador o barbero con permiso Citas/Crear.
        [HttpGet]
        public IActionResult BuscarClientes(string? q)
        {
            var usuario = UsuarioActual();
            if (usuario == null || !PuedeCrear(usuario) || EsCliente(usuario))
                return SinAcceso(usuario);

            q = (q ?? string.Empty).Trim();
            if (q.Length < 2) return Json(Array.Empty<object>());
            if (q.Length > 100) return BadRequest(new { mensaje = "Busqueda demasiado larga." });

            var consulta = _context.Clientes.AsNoTracking().AsQueryable();
            if (int.TryParse(q, out int numero))
            {
                consulta = consulta.Where(c =>
                    c.IdCliente == numero || c.Nombre.Contains(q) || c.Telefono.Contains(q));
            }
            else
            {
                consulta = consulta.Where(c => c.Nombre.Contains(q) || c.Telefono.Contains(q));
            }

            var clientes = consulta
                .OrderBy(c => c.Nombre)
                .ThenBy(c => c.IdCliente)
                .Take(10)
                .Select(c => new
                {
                    id = c.IdCliente,
                    nombre = c.Nombre,
                    telefono = c.Telefono
                }).ToList();

            return Json(clientes);
        }

        // DTO exclusivo para el registro rapido; no admite IdUsuario ni otros campos.
        public sealed class ClienteRapidoDto
        {
            public string? Nombre { get; set; }
            public string? Telefono { get; set; }
            public string? Email { get; set; }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RegistrarClienteRapido([FromBody] ClienteRapidoDto? datos)
        {
            var usuario = UsuarioActual();
            if (usuario == null || !PuedeCrear(usuario) || EsCliente(usuario))
                return SinAcceso(usuario);

            if (datos == null)
                return BadRequest(new { mensaje = "No se recibieron datos." });

            string nombre = (datos.Nombre ?? string.Empty).Trim();
            string telefono = (datos.Telefono ?? string.Empty).Trim();
            string correo = (datos.Email ?? string.Empty).Trim();

            if (nombre.Length < 3 || nombre.Length > 100)
                return BadRequest(new { mensaje = "El nombre debe tener entre 3 y 100 caracteres." });

            if (telefono.Length > 20 ||
                !System.Text.RegularExpressions.Regex.IsMatch(telefono, @"^[0-9+()\s-]+$") ||
                System.Text.RegularExpressions.Regex.Replace(telefono, @"\D", "").Length < 8)
                return BadRequest(new { mensaje = "Ingresa un telefono valido." });

            if (correo.Length > 100 ||
                (correo.Length > 0 &&
                 !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correo)))
                return BadRequest(new { mensaje = "Ingresa un correo valido." });

            // Normalizar al menos formatos frecuentes: 8888-8888 / 88888888.
            string numeroComparacion = telefono.Replace(" ", "").Replace("-", "")
                .Replace("(", "").Replace(")", "");

            bool telefonoExiste = _context.Clientes.Any(c =>
                c.Telefono.Replace(" ", "").Replace("-", "")
                    .Replace("(", "").Replace(")", "") == numeroComparacion);
            bool correoExiste = correo.Length > 0 && _context.Clientes.Any(c => c.Email == correo);

            if (telefonoExiste || correoExiste)
                return Conflict(new { mensaje = "Ya existe un cliente con ese telefono o correo. Buscalo antes de registrar otro." });

            var nuevo = new Cliente
            {
                Nombre = nombre,
                Telefono = telefono,
                Email = correo.Length == 0 ? null : correo,
                FechaRegistro = AgendaBarberoService.AhoraCostaRica()
            };

            try
            {
                _context.Clientes.Add(nuevo);
                _context.SaveChanges();
                return Json(new { id = nuevo.IdCliente, nombre = nuevo.Nombre, telefono = nuevo.Telefono });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { mensaje = "No se pudo registrar el cliente. Verifica que no exista ya." });
            }
        }
    }
}
