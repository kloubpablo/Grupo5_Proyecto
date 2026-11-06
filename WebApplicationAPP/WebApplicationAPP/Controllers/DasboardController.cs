
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;
using WebApplicationAPP.Services;

namespace WebApplicationAPP.Controllers
{
    public class DashboardController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public DashboardController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private Usuario? UsuarioActual()
        {
            int? id = HttpContext.Session.GetInt32("IdUsuario");
            if (!id.HasValue) return null;
            return _context.Usuarios.AsNoTracking().Include(u => u.IdRolNavigation)
                .FirstOrDefault(u => u.IdUsuario == id.Value && u.Estado && !u.ContraTemp &&
                    u.IdRolNavigation.Estado &&
                    (u.IdRolNavigation.Nombre == "Administrador" || u.IdRolNavigation.Nombre == "Barbero"));
        }

        private async Task<IActionResult> DashboardPersonal(Usuario barbero)
        {
            // Todas las consultas se limitan en SQL por la identidad de la sesion.
            int id = barbero.IdUsuario;
            DateOnly hoy = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica());
            DateOnly inicioSemana = hoy.AddDays(-6);
            DateOnly inicioMes = new(hoy.Year, hoy.Month, 1);

            var citas = _context.Citas.AsNoTracking().Where(c => c.IdBarbero == id);
            var atenciones = _context.Atencions.AsNoTracking().Where(a => a.IdBarbero == id);
            // Los pagos sin IdAtencion historicamente no pertenecen a un barbero identificable.
            var pagos = _context.Pagos.AsNoTracking().Where(p =>
                p.IdAtencion.HasValue && p.Atencion != null && p.Atencion.IdBarbero == id);

            var agendaHoy = await citas.Where(c => c.Fecha == hoy)
                .Include(c => c.IdClienteNavigation).OrderBy(c => c.Hora).ToListAsync();

            var fechasSemana = await citas.Where(c => c.Fecha >= inicioSemana && c.Fecha <= hoy)
                .Select(c => c.Fecha).ToListAsync();

            var vm = new DashboardBarberoVM
            {
                NombreBarbero = barbero.Nombre,
                Fecha = hoy,
                AgendaHoy = agendaHoy,
                CitasHoy = agendaHoy.Count,
                CitasActivasHoy = agendaHoy.Count(c => c.Estado == "Activa"),
                CitasCanceladasHoy = agendaHoy.Count(c => c.Estado == "Cancelada"),
                CitasProximas = await citas.CountAsync(c => c.Fecha >= hoy && c.Estado == "Activa"),
                AtencionesEnEspera = await atenciones.CountAsync(a => a.Estado == "En espera"),
                AtencionesEnServicio = await atenciones.CountAsync(a => a.Estado == "En servicio"),
                AtencionesFinalizadas = await atenciones.CountAsync(a => a.Estado == "Finalizado"),
                CitasCanceladasMes = await citas.CountAsync(c => c.Fecha >= inicioMes && c.Fecha <= hoy && c.Estado == "Cancelada"),
                PagosHoy = await pagos.CountAsync(p => p.Fecha == hoy),
                IngresosHoy = await pagos.Where(p => p.Fecha == hoy).SumAsync(p => (decimal?)p.Monto) ?? 0m,
                IngresosMes = await pagos.Where(p => p.Fecha >= inicioMes && p.Fecha <= hoy)
                    .SumAsync(p => (decimal?)p.Monto) ?? 0m
            };

            for (int i = 0; i < 7; i++)
            {
                var dia = inicioSemana.AddDays(i);
                vm.DiasGrafico.Add(dia.ToString("dd/MM"));
                vm.CantidadesGrafico.Add(fechasSemana.Count(f => f == dia));
            }

            ViewBag.ResumenBarbero = vm;
            // El dashboard preexistente lee estos datos al inicio de Razor.
            // Se suministran valores personales/ceros, nunca totales generales.
            ViewBag.CitasHoy = vm.CitasHoy;
            ViewBag.CitasActivasHoy = vm.CitasActivasHoy;
            ViewBag.CitasCanceladasHoy = vm.CitasCanceladasHoy;
            ViewBag.IngresosHoy = vm.IngresosHoy;
            ViewBag.PagosHoy = vm.PagosHoy;
            ViewBag.ClientesAtendidos = vm.AtencionesFinalizadas;
            ViewBag.TotalClientes = 0;
            ViewBag.TotalUsuarios = 0;
            ViewBag.TotalBarberos = 0;
            ViewBag.BarberosDisponibles = 0;
            ViewBag.BarberosOcupados = 0;
            ViewBag.BarberosInactivos = 0;
            ViewBag.CitasProgramadas = agendaHoy;
            ViewBag.NombresBarberosConCita = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ViewBag.DiasGrafico = vm.DiasGrafico;
            ViewBag.CantidadesGrafico = vm.CantidadesGrafico;
            ViewBag.FechaActual = AgendaBarberoService.AhoraCostaRica();

            return View("Index", new List<Usuario>());
        }

        public async Task<IActionResult> Index()
        {
            var usuario = UsuarioActual();
            if (usuario == null) return RedirectToAction("Index", "Seguridad");
            if (usuario.IdRolNavigation.Nombre == "Barbero")
            {
                if (!PermisosHelper.TienePermiso(_context, "Barbero", "Dashboard/Index"))
                    return StatusCode(403);
                return await DashboardPersonal(usuario);
            }
            if (usuario.IdRolNavigation.Nombre != "Administrador") return StatusCode(403);
            // ==========================================
            // FECHA ACTUAL
            // ==========================================

            DateOnly hoy = DateOnly.FromDateTime(AgendaBarberoService.AhoraCostaRica());

            // ==========================================
            // CITAS PROGRAMADAS PARA HOY
            // ==========================================

            var citasProgramadas = await _context.Citas
                .AsNoTracking()
                .Include(c => c.IdClienteNavigation)
                .Where(c => c.Fecha == hoy)
                .OrderBy(c => c.Hora)
                .ToListAsync();

            int citasHoy = citasProgramadas.Count;

            int citasActivasHoy = citasProgramadas.Count(
                c => string.Equals(
                    c.Estado,
                    "Activa",
                    StringComparison.OrdinalIgnoreCase
                )
            );

            int citasCanceladasHoy = citasProgramadas.Count(
                c => string.Equals(
                    c.Estado,
                    "Cancelada",
                    StringComparison.OrdinalIgnoreCase
                )
            );

            // ==========================================
            // INGRESOS DEL DIA
            // Se calculan desde los pagos registrados.
            // ==========================================

            decimal ingresosHoy = await _context.Pagos
                .AsNoTracking()
                .Where(p => p.Fecha == hoy)
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

            int pagosHoy = await _context.Pagos
                .AsNoTracking()
                .CountAsync(p => p.Fecha == hoy);

            // ==========================================
            // CLIENTES ATENDIDOS
            // Total historico de atenciones finalizadas.
            // ==========================================

            int clientesAtendidos = await _context.Atencions
                .AsNoTracking()
                .CountAsync(a => a.Estado == "Finalizado");

            // ==========================================
            // RESUMEN DEL SISTEMA
            // ==========================================

            int totalClientes = await _context.Clientes
                .AsNoTracking()
                .CountAsync();

            int totalUsuarios = await _context.Usuarios
                .AsNoTracking()
                .CountAsync();

            // ==========================================
            // BARBEROS
            // IdRol 2 es el rol de barbero en el codigo
            // original de este proyecto.
            // ==========================================

            var barberos = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.IdRolNavigation.Nombre == "Barbero")
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            int totalBarberos = barberos.Count;

            int barberosInactivos = barberos.Count(
                b => !b.Estado
            );

            // ==========================================
            // BARBEROS CON CITAS ACTIVAS HOY
            // ==========================================

            var nombresBarberosConCita = new HashSet<string>(
                citasProgramadas
                    .Where(c =>
                        string.Equals(
                            c.Estado,
                            "Activa",
                            StringComparison.OrdinalIgnoreCase
                        )
                        && !string.IsNullOrWhiteSpace(c.Barbero)
                    )
                    .Select(c => c.Barbero.Trim()),
                StringComparer.OrdinalIgnoreCase
            );

            int barberosOcupados = barberos.Count(b =>
                b.Estado &&
                !string.IsNullOrWhiteSpace(b.Nombre) &&
                nombresBarberosConCita.Contains(
                    b.Nombre.Trim()
                )
            );

            int barberosDisponibles = barberos.Count(b =>
                b.Estado &&
                (
                    string.IsNullOrWhiteSpace(b.Nombre) ||
                    !nombresBarberosConCita.Contains(
                        b.Nombre.Trim()
                    )
                )
            );

            // ==========================================
            // GRAFICO: CITAS DE LOS ULTIMOS 7 DIAS
            // ==========================================

            DateOnly fechaInicio = hoy.AddDays(-6);

            var fechasCitas = await _context.Citas
                .AsNoTracking()
                .Where(c =>
                    c.Fecha >= fechaInicio &&
                    c.Fecha <= hoy
                )
                .Select(c => c.Fecha)
                .ToListAsync();

            var diasGrafico = new List<string>();
            var cantidadesGrafico = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                DateOnly fecha = fechaInicio.AddDays(i);

                diasGrafico.Add(
                    fecha.ToString("dd/MM")
                );

                cantidadesGrafico.Add(
                    fechasCitas.Count(f => f == fecha)
                );
            }

            // ==========================================
            // VIEWBAG - ESTADISTICAS
            // ==========================================

            ViewBag.CitasHoy = citasHoy;
            ViewBag.CitasActivasHoy = citasActivasHoy;
            ViewBag.CitasCanceladasHoy = citasCanceladasHoy;

            ViewBag.IngresosHoy = ingresosHoy;
            ViewBag.PagosHoy = pagosHoy;

            ViewBag.ClientesAtendidos = clientesAtendidos;

            ViewBag.TotalClientes = totalClientes;
            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalBarberos = totalBarberos;

            ViewBag.BarberosDisponibles = barberosDisponibles;
            ViewBag.BarberosOcupados = barberosOcupados;
            ViewBag.BarberosInactivos = barberosInactivos;

            // ==========================================
            // VIEWBAG - LISTADOS Y GRAFICO
            // ==========================================

            ViewBag.CitasProgramadas = citasProgramadas;

            ViewBag.NombresBarberosConCita =
                nombresBarberosConCita;

            ViewBag.DiasGrafico = diasGrafico;
            ViewBag.CantidadesGrafico = cantidadesGrafico;

            ViewBag.FechaActual = AgendaBarberoService.AhoraCostaRica();

            // ==========================================
            // MOSTRAR DASHBOARD
            // ==========================================

            return View(barberos);
        }
    }
}
