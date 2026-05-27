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

        // 🔥 ADMIN Y RECEPCIONISTA
        private bool TieneAcceso()
        {
            var rol = HttpContext.Session
                .GetString("Rol");

            return rol == "Administrador"
                || rol == "Recepcionista";
        }

        // 🔥 LISTA DE CLIENTES
        public IActionResult Index(string buscar)
        {
            if (!TieneAcceso())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var clientes = _context.Clientes.AsQueryable();

            // 🔍 BUSCAR CLIENTE
            if (!string.IsNullOrEmpty(buscar))
            {
                clientes = clientes.Where(c =>
                    c.Nombre.Contains(buscar));
            }

            return View(clientes.ToList());
        }

        // 🔥 CREAR CLIENTE (GET)
        public IActionResult Crear()
        {
            if (!TieneAcceso())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View();
        }

        // 🔥 CREAR CLIENTE (POST)
        [HttpPost]
        public IActionResult Crear(
            string nombre,
            string telefono)
        {
            if (!TieneAcceso())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // 🔥 VALIDAR CAMPOS
            if (string.IsNullOrEmpty(nombre) ||
                string.IsNullOrEmpty(telefono))
            {
                ViewBag.Error =
                    "Debe completar todos los campos";

                return View();
            }

            // 🔥 VALIDAR DUPLICADO
            bool existe = _context.Clientes
                .Any(c => c.Nombre == nombre);

            if (existe)
            {
                ViewBag.Error =
                    "El cliente ya existe";

                return View();
            }

            // 🔥 CREAR CLIENTE
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

        // 🔥 EDITAR CLIENTE (GET)
        public IActionResult Editar(int id)
        {
            if (!TieneAcceso())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var cliente = _context.Clientes
                .FirstOrDefault(c =>
                    c.IdCliente == id);

            if (cliente == null)
            {
                return RedirectToAction("Index");
            }

            return View(cliente);
        }

        // 🔥 EDITAR CLIENTE (POST)
        [HttpPost]
        public IActionResult Editar(
            int id,
            string nombre,
            string telefono)
        {
            if (!TieneAcceso())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var cliente = _context.Clientes
                .FirstOrDefault(c =>
                    c.IdCliente == id);

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