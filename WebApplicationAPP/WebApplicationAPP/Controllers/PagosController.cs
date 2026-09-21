using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;

namespace WebApplicationAPP.Controllers
{
    public class PagosController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public PagosController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
                return false;

            return PermisosHelper.TienePermiso(_context, rol, permiso);
        }

        //LISTA DE PAGOS
        public IActionResult Index()
        {
            if (!TienePermiso("Pagos/Index"))
                return RedirectToAction("Index", "Dashboard");

            var pagos = _context.Pagos
                .Include(p => p.IdClienteNavigation)
                .ToList();

            return View(pagos);
        }

        //REGISTRAR (GET)
        public IActionResult Registrar(int? idAtencion)
        {
            if (!TienePermiso("Pagos/Crear"))
                return RedirectToAction("Index", "Dashboard");

            ViewBag.IdAtencion = idAtencion;

            //Si el pago viene desde una atención,
            //se obtiene automáticamente el cliente.
            if (idAtencion.HasValue)
            {
                var atencion = _context.Atencions
                    .Include(a => a.IdClienteNavigation)
                    .FirstOrDefault(a => a.IdAtencion == idAtencion.Value);

                if (atencion == null)
                    return RedirectToAction("Index", "Atencion");

                if (atencion.Estado != "En servicio")
                {
                    return RedirectToAction("Index", "Atencion");
                }

                ViewBag.ClienteAtencion = atencion.IdClienteNavigation.Nombre;
            }

            return View();
        }

        //REGISTRAR (POST)
        [HttpPost]
        public IActionResult Registrar(
            string cliente,
            decimal monto,
            string metodo,
            int? idAtencion)
        {
            if (!TienePermiso("Pagos/Crear"))
                return RedirectToAction("Index", "Dashboard");

            ViewBag.IdAtencion = idAtencion;

            //Si viene desde una atención,
            //obtenemos nuevamente el cliente desde la BD.
            Atencion? atencion = null;

            if (idAtencion.HasValue)
            {
                atencion = _context.Atencions
                    .Include(a => a.IdClienteNavigation)
                    .FirstOrDefault(a => a.IdAtencion == idAtencion.Value);

                if (atencion == null)
                    return RedirectToAction("Index", "Atencion");

                if (atencion.Estado != "En servicio")
                {
                    return RedirectToAction("Index", "Atencion");
                }

                cliente = atencion.IdClienteNavigation.Nombre;
                ViewBag.ClienteAtencion = cliente;
            }

            if (string.IsNullOrWhiteSpace(cliente) ||
                monto <= 0 ||
                string.IsNullOrWhiteSpace(metodo))
            {
                ViewBag.Error = "Debe completar todos los datos correctamente";
                return View();
            }

            var clienteExistente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == cliente);

            if (clienteExistente == null)
            {
                //Este comportamiento se mantiene para los pagos
                //registrados manualmente.
                clienteExistente = new Cliente
                {
                    Nombre = cliente,
                    Telefono = "00000000",
                    FechaRegistro = DateTime.Now
                };

                _context.Clientes.Add(clienteExistente);
                _context.SaveChanges();
            }

            var pago = new Pago
            {
                IdCliente = clienteExistente.IdCliente,
                Monto = monto,
                Metodo = metodo,
                Fecha = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Pagos.Add(pago);

            //Si el pago pertenece a una atención,
            //finalizamos la atención y, si tiene cita,
            //también finalizamos la cita.
            if (atencion != null)
            {
                atencion.Estado = "Finalizado";
                atencion.HoraFin = TimeOnly.FromDateTime(DateTime.Now);

                if (atencion.IdCita.HasValue)
                {
                    var cita = _context.Citas
                        .FirstOrDefault(c => c.IdCita == atencion.IdCita.Value);

                    if (cita != null)
                    {
                        cita.Estado = "Finalizada";
                    }
                }
            }

            _context.SaveChanges();

            if (atencion != null)
            {
                return RedirectToAction("Index", "Atencion");
            }

            return RedirectToAction("Index");
        }

        //CIERRE DE CAJA
        public IActionResult Cierre()
        {
            if (!TienePermiso("Pagos/Index"))
                return RedirectToAction("Index", "Dashboard");


            var hoy = DateOnly.FromDateTime(DateTime.Now);


            var pagosHoy = _context.Pagos
                .Where(p => p.Fecha == hoy && !p.Cerrado)
                .ToList();


            ViewBag.Fecha = DateTime.Now.ToString("dd/MM/yyyy");

            ViewBag.Total = pagosHoy.Sum(p => p.Monto);

            ViewBag.CantidadPagos = pagosHoy.Count;


            ViewBag.MetodoMasUsado = pagosHoy
                .GroupBy(p => p.Metodo)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "Sin registros";


            ViewBag.TotalEfectivo = pagosHoy
                .Where(p => p.Metodo == "Efectivo")
                .Sum(p => p.Monto);


            ViewBag.TotalTarjeta = pagosHoy
                .Where(p => p.Metodo == "Tarjeta")
                .Sum(p => p.Monto);


            return View();
        }

        [HttpPost]
        public IActionResult AplicarCierre()
        {

            if (!TienePermiso("Pagos/Index"))
                return RedirectToAction("Index", "Dashboard");


            var hoy = DateOnly.FromDateTime(DateTime.Now);


            var pagos = _context.Pagos
                .Where(p => p.Fecha == hoy && !p.Cerrado)
                .ToList();


            foreach (var pago in pagos)
            {
                pago.Cerrado = true;
            }


            _context.SaveChanges();


            return RedirectToAction("Cierre");
        }
    }
}