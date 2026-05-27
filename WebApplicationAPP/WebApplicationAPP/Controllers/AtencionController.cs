using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class AtencionController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public AtencionController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private bool EsBarbero()
        {
            return HttpContext.Session
                .GetString("Rol") == "Barbero";
        }

        public IActionResult Index()
        {
            if (!EsBarbero())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var atenciones = _context.Atencions
                .Include(a => a.IdClienteNavigation)
                .ToList();

            return View(atenciones);
        }

        [HttpPost]
        public IActionResult Agregar(string nombre)
        {
            if (!EsBarbero())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            if (string.IsNullOrEmpty(nombre))
            {
                return RedirectToAction("Index");
            }

            var cliente = _context.Clientes
                .FirstOrDefault(c =>
                    c.Nombre == nombre);

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
            if (!EsBarbero())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var atencion = _context.Atencions
                .FirstOrDefault(a =>
                    a.IdAtencion == id);

            if (atencion != null)
            {
                atencion.Estado = "En servicio";

                atencion.HoraInicio =
                    TimeOnly.FromDateTime(DateTime.Now);

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Finalizar(int id)
        {
            if (!EsBarbero())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var atencion = _context.Atencions
                .FirstOrDefault(a =>
                    a.IdAtencion == id);

            if (atencion != null)
            {
                atencion.Estado = "Finalizado";

                atencion.HoraFin =
                    TimeOnly.FromDateTime(DateTime.Now);

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}