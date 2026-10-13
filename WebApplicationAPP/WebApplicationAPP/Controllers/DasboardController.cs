
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class DashboardController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public DashboardController(YampiBarbershopContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ==========================================
            // FECHA ACTUAL
            // ==========================================

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);

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
                .Where(u => u.IdRol == 2)
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

            ViewBag.FechaActual = DateTime.Now;

            // ==========================================
            // MOSTRAR DASHBOARD
            // ==========================================

            return View(barberos);
        }
    }
}
