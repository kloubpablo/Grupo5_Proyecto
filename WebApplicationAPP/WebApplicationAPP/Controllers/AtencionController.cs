using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Helpers;
using WebApplicationAPP.Models;
using WebApplicationAPP.Services;

namespace WebApplicationAPP.Controllers;

// Un solo punto para la agenda y el ciclo de vida de la atencion.
// Las citas reservadas se leen de Citas; solo se crea Atencion al registrar la llegada.
public class AtencionController : Controller
{
    private readonly YampiBarbershopContext _db;
    public AtencionController(YampiBarbershopContext db) => _db = db;

    private Usuario? Actual()
    {
        int? id = HttpContext.Session.GetInt32("IdUsuario");
        if (!id.HasValue) return null;
        return _db.Usuarios.Include(u => u.IdRolNavigation).AsNoTracking()
            .FirstOrDefault(u => u.IdUsuario == id && u.Estado && !u.ContraTemp &&
                u.IdRolNavigation.Estado &&
                (u.IdRolNavigation.Nombre == "Administrador" || u.IdRolNavigation.Nombre == "Barbero"));
    }

    private bool Permiso(Usuario u, string clave) =>
        AgendaBarberoService.EsAdministrador(u) ||
        PermisosHelper.TienePermiso(_db, u, clave);

    private IActionResult SinAcceso(Usuario? u) =>
        u == null ? RedirectToAction("Index", "Seguridad") : StatusCode(403);

    [HttpGet]
    public IActionResult Index()
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Atencion/Index")) return SinAcceso(u);

        bool admin = AgendaBarberoService.EsAdministrador(u);
        var ahora = AgendaBarberoService.AhoraCostaRica();
        var hoy = DateOnly.FromDateTime(ahora);

        var query = _db.Atencions.AsNoTracking()
            .Include(a => a.IdClienteNavigation)
            .Include(a => a.IdCitaNavigation).AsQueryable();
        if (!admin) query = query.Where(a => a.IdBarbero == u.IdUsuario);

        // Historial limitado para no cargar indefinidamente la pagina.
        var activos = query.Where(a => a.Estado == "En espera" || a.Estado == "En servicio")
            .OrderBy(a => a.IdAtencion).ToList();
        var historial = Permiso(u, "Atencion/Historial")
            ? query.Where(a => a.Estado == "Finalizado" || a.Estado == "Cancelado")
                .OrderByDescending(a => a.IdAtencion).Take(150).ToList()
            : new List<Atencion>();

        var agendas = _db.Citas.AsNoTracking()
            .Include(c => c.IdClienteNavigation)
            .Where(c => c.Estado == "Activa" && c.Fecha >= hoy &&
                !_db.Atencions.Any(a => a.IdCita == c.IdCita));
        if (!admin) agendas = agendas.Where(c => c.IdBarbero == u.IdUsuario);

        ViewBag.CitasAgenda = agendas.OrderBy(c => c.Fecha).ThenBy(c => c.Hora)
            .Take(500).ToList();
        ViewBag.EsAdministrador = admin;
        ViewBag.PuedeAgregar = Permiso(u, "Atencion/Agregar");
        ViewBag.PuedeIniciar = Permiso(u, "Atencion/Iniciar");
        ViewBag.PuedeRecibir = Permiso(u, "Atencion/RegistrarDesdeCita");
        ViewBag.PuedeCobrar = Permiso(u, "Pagos/Crear");
        ViewBag.PuedeCancelarIngreso = Permiso(u, "Atencion/Cancelar");
        ViewBag.VerHistorial = Permiso(u, "Atencion/Historial");
        ViewBag.Hoy = hoy;
        ViewBag.HoraActual = TimeOnly.FromDateTime(ahora);
        ViewBag.MiBarberoId = admin ? (int?)null : u.IdUsuario;
        ViewBag.Barberos = admin ? _db.Usuarios.AsNoTracking()
            .Where(b => b.Estado && !b.ContraTemp && b.IdRolNavigation.Estado &&
                b.IdRolNavigation.Nombre == "Barbero")
            .OrderBy(b => b.Nombre).ToList() : new List<Usuario>();
        return View(activos.Concat(historial).ToList());
    }

    // Consulta de disponibilidad para ingreso presencial. Solo muestra horas.
    [HttpGet]
    public IActionResult HorariosOcupados(DateOnly fecha, int? idBarbero)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Atencion/Agregar")) return SinAcceso(u);
        int id = AgendaBarberoService.EsAdministrador(u) ? (idBarbero ?? 0) : u.IdUsuario;
        var barbero = AgendaBarberoService.BuscarBarbero(_db, id);
        if (barbero == null) return BadRequest(new { error = "Selecciona un barbero valido." });
        var hoy = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica());
        if (fecha != hoy) return BadRequest(new { error = "El ingreso presencial se realiza hoy." });
        var ocupadas = _db.Citas.AsNoTracking().Where(c => c.Fecha == fecha &&
            (c.Estado == "Activa" || c.Estado == "Bloqueada") &&
            (c.IdBarbero == id || (c.IdBarbero == null && c.Barbero == barbero.Nombre)))
            .Select(c => c.Hora).ToList();
        return Json(new
        {
            horasOcupadas = ocupadas.Select(h => h.ToString("HH:mm")).ToList(),
            horariosPermitidos = Enumerable.Range(0, 28)
                .Select(i => new TimeOnly(7, 0).AddMinutes(i * 30))
                .Where(h => AgendaBarberoService.EsHorarioValido(fecha, h, ingresoPresencial: true) &&
                    !ocupadas.Contains(h))
                .Select(h => h.ToString("HH:mm")).ToList()
        });
    }

    // Cliente que llega sin reserva: crea Cita + Atencion EN LA MISMA TRANSACCION.
    // La cita ocupa de inmediato un turno real visible para reservas web.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Agregar(string nombre, string? telefono, DateOnly fecha, TimeOnly hora, int? idBarbero)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Atencion/Agregar")) return SinAcceso(u);
        bool admin = AgendaBarberoService.EsAdministrador(u);
        int barberoId = admin ? idBarbero ?? 0 : u.IdUsuario;
        var barbero = AgendaBarberoService.BuscarBarbero(_db, barberoId);
        var hoy = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica());

        nombre = (nombre ?? "").Trim();
        telefono = (telefono ?? "").Trim();
        if (nombre.Length is < 2 or > 100 || telefono.Length > 20 ||
            barbero == null || fecha != hoy ||
            !AgendaBarberoService.EsHorarioValido(fecha, hora, ingresoPresencial: true))
        {
            TempData["ErrorAtencion"] = "Selecciona un barbero, un cliente y un turno valido de hoy (7:00 a 20:30).";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var tx = _db.Database.BeginTransaction(IsolationLevel.Serializable);
            if (AgendaBarberoService.Ocupado(_db, barberoId, barbero.Nombre, fecha, hora))
            {
                TempData["ErrorAtencion"] = "Ese horario ya esta reservado para el barbero.";
                return RedirectToAction(nameof(Index));
            }

            // No buscar por nombre: dos clientes con nombre igual pueden ser distintos.
            var cliente = new Cliente
            {
                Nombre = nombre,
                Telefono = string.IsNullOrWhiteSpace(telefono) ? "00000000" : telefono,
                FechaRegistro = AgendaBarberoService.AhoraCostaRica()
            };
            _db.Clientes.Add(cliente);
            _db.SaveChanges();

            var cita = new Cita
            {
                IdCliente = cliente.IdCliente,
                IdBarbero = barberoId,
                Barbero = barbero.Nombre, // compatibilidad de reportes/vistas antiguos
                Fecha = fecha,
                Hora = hora,
                Estado = "Activa"
            };
            _db.Citas.Add(cita);
            _db.SaveChanges();

            _db.Atencions.Add(new Atencion
            {
                IdCliente = cliente.IdCliente,
                IdBarbero = barberoId,
                IdCita = cita.IdCita,
                Estado = "En espera"
            });
            _db.SaveChanges();
            tx.Commit();
            TempData["ExitoAtencion"] = "Cliente registrado y horario bloqueado en la agenda.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorAtencion"] = "No se pudo reservar ese turno; probablemente otro usuario lo reservo al mismo tiempo.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Registrar llegada del cliente con cita. POST idempotente ante dobles clics.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult RegistrarDesdeCita(int id)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Atencion/RegistrarDesdeCita")) return SinAcceso(u);
        using var tx = _db.Database.BeginTransaction(IsolationLevel.Serializable);
        var cita = _db.Citas.FirstOrDefault(c => c.IdCita == id);
        if (cita == null) return NotFound();
        if (!AgendaBarberoService.PuedeGestionar(u, cita.IdBarbero)) return StatusCode(403);
        if (cita.Estado != "Activa") return BadRequest("La cita no esta activa.");
        var hoy = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica());
        if (cita.Fecha != hoy) return BadRequest("La llegada solo se registra el dia de la cita.");

        try
        {
            if (_db.Atencions.Any(a => a.IdCita == id))
            {
                TempData["ExitoAtencion"] = "El cliente ya esta registrado en atencion.";
                return RedirectToAction(nameof(Index));
            }
            _db.Atencions.Add(new Atencion
            {
                IdCliente = cita.IdCliente,
                IdCita = cita.IdCita,
                IdBarbero = cita.IdBarbero,
                Estado = "En espera"
            });
            _db.SaveChanges();
            tx.Commit();
            TempData["ExitoAtencion"] = "Cliente registrado en espera.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorAtencion"] = "Esta cita ya fue recibida o no puede registrarse.";
        }
        return RedirectToAction(nameof(Index));
    }

    // Retiro de cliente en espera: se cancela la cita para liberar su turno.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cancelar(int id)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Atencion/Cancelar")) return SinAcceso(u);
        using var tx = _db.Database.BeginTransaction(IsolationLevel.Serializable);
        var atencion = _db.Atencions.Include(a => a.IdCitaNavigation)
            .FirstOrDefault(a => a.IdAtencion == id);
        if (atencion == null) return NotFound();
        if (!AgendaBarberoService.PuedeGestionar(u, atencion.IdBarbero)) return StatusCode(403);
        if (atencion.Estado != "En espera") return BadRequest("Solo se pueden retirar ingresos en espera.");
        if (_db.Pagos.Any(p => p.IdAtencion == id))
            return BadRequest("Una atencion con pago no puede cancelarse.");
        atencion.Estado = "Cancelado";
        if (atencion.IdCitaNavigation != null && atencion.IdCitaNavigation.Estado == "Activa")
            atencion.IdCitaNavigation.Estado = "Cancelada";
        _db.SaveChanges();
        tx.Commit();
        TempData["ExitoAtencion"] = "Ingreso retirado y turno liberado.";
        return RedirectToAction(nameof(Index));
    }

    // POST, nunca GET: una visita a un enlace no puede iniciar un servicio.
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Iniciar(int id)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Atencion/Iniciar")) return SinAcceso(u);
        using var tx = _db.Database.BeginTransaction(IsolationLevel.Serializable);
        var atencion = _db.Atencions.Include(a => a.IdCitaNavigation)
            .FirstOrDefault(a => a.IdAtencion == id);
        if (atencion == null) return NotFound();
        if (!AgendaBarberoService.PuedeGestionar(u, atencion.IdBarbero)) return StatusCode(403);
        if (atencion.Estado != "En espera") return BadRequest("La atencion no esta en espera.");

        var ahora = AgendaBarberoService.AhoraCostaRica();
        if (atencion.IdCitaNavigation != null)
        {
            var cita = atencion.IdCitaNavigation;
            if (cita.Estado != "Activa" || cita.Fecha != DateOnly.FromDateTime(ahora) ||
                cita.Fecha.ToDateTime(cita.Hora) > ahora)
                return BadRequest("El servicio solo puede comenzar cuando llega su turno.");
        }

        try
        {
            if (atencion.IdBarbero.HasValue && _db.Atencions.Any(a =>
                a.IdBarbero == atencion.IdBarbero && a.IdAtencion != atencion.IdAtencion &&
                a.Estado == "En servicio"))
                return BadRequest("El barbero ya tiene una atencion en servicio.");
            atencion.Estado = "En servicio";
            atencion.HoraInicio = TimeOnly.FromDateTime(ahora);
            _db.SaveChanges();
            tx.Commit();
            TempData["ExitoAtencion"] = "Atencion iniciada.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorAtencion"] = "No se pudo iniciar: revisa si el barbero ya esta ocupado.";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET compatible: solo muestra el formulario de cobro, no modifica datos.
    [HttpGet]
    public IActionResult Finalizar(int id)
    {
        var u = Actual();
        if (u == null || !Permiso(u, "Pagos/Crear")) return SinAcceso(u);
        var atencion = _db.Atencions.AsNoTracking().FirstOrDefault(a => a.IdAtencion == id);
        if (atencion == null) return NotFound();
        if (!AgendaBarberoService.PuedeGestionar(u, atencion.IdBarbero)) return StatusCode(403);
        if (atencion.Estado != "En servicio") return BadRequest("La atencion no esta en servicio.");
        return RedirectToAction("Registrar", "Pagos", new { idAtencion = id });
    }
}
