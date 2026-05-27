using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class RolesController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public RolesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // 🔥 VALIDAR SI ES ADMINISTRADOR
        private bool EsAdmin()
        {
            return HttpContext.Session
                .GetString("Rol") == "Administrador";
        }

        // 🔥 INDEX
        public IActionResult Index()
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var roles = _context.Roles.ToList();

            return View(roles);
        }

        // 🔥 CREAR GET
        public IActionResult Crear()
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View();
        }

        // 🔥 CREAR POST
        [HttpPost]
        public IActionResult Crear(string nombre, string descripcion)
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // VALIDAR CAMPOS
            if (string.IsNullOrEmpty(nombre))
            {
                ViewBag.Error =
                    "Debe ingresar el nombre del rol";

                return View();
            }

            // VALIDAR DUPLICADO
            bool existe = _context.Roles
                .Any(r => r.Nombre == nombre);

            if (existe)
            {
                ViewBag.Error =
                    "El rol ya existe";

                return View();
            }

            // CREAR ROL
            var rol = new Role
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Estado = true
            };

            _context.Roles.Add(rol);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 🔥 ACTIVAR / DESACTIVAR
        public IActionResult Toggle(int id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var rol = _context.Roles
                .FirstOrDefault(r =>
                    r.IdRol == id);

            if (rol != null)
            {
                rol.Estado = !rol.Estado;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // 🔥 ELIMINAR
        public IActionResult Eliminar(int id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var rol = _context.Roles
                .FirstOrDefault(r =>
                    r.IdRol == id);

            if (rol != null)
            {
                _context.Roles.Remove(rol);

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}