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

        // LISTA DE ATENCIONES
        public IActionResult Index()
        {
            var atenciones = _context.Atencions
                .Include(a => a.IdClienteNavigation)
                .ToList();

            return View(atenciones);
        }

        // AGREGAR CLIENTE A LISTA
        [HttpPost]
        public IActionResult Agregar(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
            {
                return RedirectToAction("Index");
            }

            // Buscar cliente existente
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == nombre);

            // Si no existe, crearlo
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

            // Crear atención
            var atencion = new Atencion
            {
                IdCliente = cliente.IdCliente,
                Estado = "En espera"
            };

            _context.Atencions.Add(atencion);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // INICIAR SERVICIO
        public IActionResult Iniciar(int id)
        {
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

        // FINALIZAR SERVICIO
        public IActionResult Finalizar(int id)
        {
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
    }
}