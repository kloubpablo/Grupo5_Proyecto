using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationAPP.Controllers
{
    public class ClientesController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public ClientesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        //PERMISOS CENTRALIZADO
        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol") ?? "";
            return PermisosHelper.TienePermiso(_context, rol, permiso);
        }

        //LISTA DE CLIENTES
        public IActionResult Index(string buscar)
        {
            if (!TienePermiso("Clientes/Index"))
                return RedirectToAction("Index", "Dashboard");

            var clientes = _context.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                clientes = clientes.Where(c =>
                    c.Nombre.Contains(buscar) ||
                    c.Telefono.Contains(buscar));
            }

            return View(clientes.ToList());
        }

        //CREAR CLIENTE 
        public IActionResult Crear()
        {
            if (!TienePermiso("Clientes/Crear"))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // CREAR CLIENTE///
        [HttpPost]
        public IActionResult Crear(string nombre, string telefono)
        {
            if (!TienePermiso("Clientes/Crear"))
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(telefono))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            bool existe = _context.Clientes
                .Any(c => c.Nombre == nombre);

            if (existe)
            {
                ViewBag.Error = "El cliente ya existe";
                return View();
            }

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

        ///EDITAR CLIENTE////
        public IActionResult Editar(int id)
        {
            if (!TienePermiso("Clientes/Editar"))
                return RedirectToAction("Index", "Dashboard");

            var cliente = _context.Clientes
                .FirstOrDefault(c => c.IdCliente == id);

            if (cliente == null)
                return RedirectToAction("Index");

            return View(cliente);
        }

        ///EDITAR CLIENTE///////
        [HttpPost]
        public IActionResult Editar(int id, string nombre, string telefono)
        {
            if (!TienePermiso("Clientes/Editar"))
                return RedirectToAction("Index", "Dashboard");

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