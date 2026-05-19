using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class ClientesController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public ClientesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // LISTA
        public IActionResult Index(string buscar)
        {
            var clientes = _context.Clientes.AsQueryable();

            // BUSCAR CLIENTE
            if (!string.IsNullOrEmpty(buscar))
            {
                clientes = clientes.Where(c =>
                    c.Nombre.Contains(buscar));
            }

            return View(clientes.ToList());
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string nombre, string telefono)
        {
            // VALIDAR CAMPOS
            if (string.IsNullOrEmpty(nombre) ||
                string.IsNullOrEmpty(telefono))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // VALIDAR DUPLICADO
            bool existe = _context.Clientes
                .Any(c => c.Nombre == nombre);

            if (existe)
            {
                ViewBag.Error = "El cliente ya existe";
                return View();
            }

            // CREAR CLIENTE
            var cliente = new Cliente
            {
                Nombre = nombre,
                Telefono = telefono,
                FechaRegistro = DateTime.Now
            };

            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // EDITAR (GET)
        public IActionResult Editar(int id)
        {
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.IdCliente == id);

            if (cliente == null)
            {
                return RedirectToAction("Index");
            }

            return View(cliente);
        }

        // EDITAR (POST)
        [HttpPost]
        public IActionResult Editar(int id, string nombre, string telefono)
        {
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.IdCliente == id);

            if (cliente != null)
            {
                cliente.Nombre = nombre;
                cliente.Telefono = telefono;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}