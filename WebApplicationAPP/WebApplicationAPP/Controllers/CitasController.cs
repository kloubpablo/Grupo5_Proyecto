using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class CitasController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public CitasController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // LISTA DE CITAS
        public IActionResult Index()
        {
            var citas = _context.Citas
                .Include(c => c.IdClienteNavigation)
                .ToList();

            return View(citas);
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string cliente, DateOnly fecha, TimeOnly hora, string barbero)
        {
            // Validar campos
            if (string.IsNullOrEmpty(cliente) || string.IsNullOrEmpty(barbero))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // Validar horario ocupado
            bool ocupado = _context.Citas.Any(c =>
                c.Fecha == fecha &&
                c.Hora == hora &&
                c.Barbero == barbero);

            if (ocupado)
            {
                ViewBag.Error = "Horario no disponible";
                return View();
            }

            // Buscar cliente
            var clienteExistente = _context.Clientes
                .FirstOrDefault(c => c.Nombre == cliente);

            // Crear cliente si no existe
            if (clienteExistente == null)
            {
                clienteExistente = new Cliente
                {
                    Nombre = cliente,
                    Telefono = "00000000",
                    FechaRegistro = DateTime.Now
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

            return RedirectToAction("Index");
        }

        // CANCELAR CITA
        public IActionResult Cancelar(int id)
        {
            var cita = _context.Citas
                .FirstOrDefault(c => c.IdCita == id);

            if (cita != null)
            {
                cita.Estado = "Cancelada";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}