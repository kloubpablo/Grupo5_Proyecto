using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class RolesController : Controller
    {
        // Lista simulada
        static List<dynamic> roles = new List<dynamic>();

        // LISTA
        public IActionResult Index()
        {
            return View(roles);
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string nombre)
        {
            // 🔴 Validación
            if (string.IsNullOrEmpty(nombre))
            {
                ViewBag.Error = "Debe ingresar el nombre del rol";
                return View();
            }

            // 🔴 Duplicado
            if (roles.Any(r => r.nombre == nombre))
            {
                ViewBag.Error = "El rol ya existe";
                return View();
            }

            roles.Add(new
            {
                id = roles.Count + 1,
                nombre = nombre,
                activo = true
            });

            return RedirectToAction("Index");
        }

        // ACTIVAR / DESACTIVAR
        public IActionResult Toggle(int id)
        {
            var rol = roles.FirstOrDefault(r => r.id == id);

            if (rol != null)
            {
                roles.Remove(rol);

                roles.Add(new
                {
                    id = rol.id,
                    nombre = rol.nombre,
                    activo = !rol.activo
                });
            }

            return RedirectToAction("Index");
        }

        // ELIMINAR
        public IActionResult Eliminar(int id)
        {
            var rol = roles.FirstOrDefault(r => r.id == id);

            if (rol != null)
            {
                roles.Remove(rol);
            }

            return RedirectToAction("Index");
        }
    }
}