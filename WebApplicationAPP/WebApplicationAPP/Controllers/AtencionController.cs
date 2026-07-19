using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;

namespace WebApplicationAPP.Controllers
{
    public class AtencionController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public AtencionController(YampiBarbershopContext context)
        {
            _context = context;
        }

        //SISTEMA DE PERMISOS
        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            return PermisosHelper.TienePermiso(_context, rol, permiso);
        }

        public IActionResult Index()
        {
            if (!TienePermiso("Atencion/Index"))
                return RedirectToAction("Index", "Dashboard");

            var atenciones = _context.Atencions
                .Include(a => a.IdClienteNavigation)
                .ToList();

            return View(atenciones);
        }

        [HttpPost]
        public IActionResult Agregar(string nombre)
        {
            if (!TienePermiso("Atencion/Index"))
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrWhiteSpace(nombre))
                return RedirectToAction("Index");

            var cliente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == nombre);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nombre = nombre,
                    Telefono = "00000000",
                    FechaRegistro = DateTime.Now
                };

                _context.Clientes.Add(cliente);
                _context.SaveChanges();
            }

            var atencion = new Atencion
            {
                IdCliente = cliente.IdCliente,
                Estado = "En espera"
            };

            _context.Atencions.Add(atencion);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Iniciar(int id)
        {
            if (!TienePermiso("Atencion/Index"))
                return RedirectToAction("Index", "Dashboard");

            var atencion = _context.Atencions
                .FirstOrDefault(a => a.IdAtencion == id);

            if (atencion != null)
            {
                atencion.Estado = "En servicio";
                atencion.HoraInicio = TimeOnly.FromDateTime(DateTime.Now);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Finalizar(int id)
        {
            if (!TienePermiso("Atencion/Index"))
                return RedirectToAction("Index", "Dashboard");

            var atencion = _context.Atencions
                .FirstOrDefault(a => a.IdAtencion == id);

            if (atencion != null)
            {
                atencion.Estado = "Finalizado";
                atencion.HoraFin = TimeOnly.FromDateTime(DateTime.Now);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        //REGISTRAR ATENCIÓN DESDE UNA CITA
        public IActionResult RegistrarDesdeCita(int id)
        {
            if (!TienePermiso("Atencion/Index"))
                return RedirectToAction("Index", "Dashboard");

            var cita = _context.Citas
                .FirstOrDefault(c => c.IdCita == id);

            if (cita == null)
                return RedirectToAction("Index", "Citas");

            bool existe = _context.Atencions
                .Any(a => a.IdCita == id);

            if (existe)
                return RedirectToAction("Index");

            var atencion = new Atencion
            {
                IdCliente = cita.IdCliente,
                IdCita = cita.IdCita,
                Estado = "En espera"
            };

            _context.Atencions.Add(atencion);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}