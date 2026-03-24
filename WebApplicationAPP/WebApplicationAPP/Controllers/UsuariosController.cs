using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class UsuariosController : Controller
    {
        // Lista simulada
        static List<dynamic> usuarios = new List<dynamic>();

        // LISTA
        public IActionResult Index()
        {
            return View(usuarios);
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string nombre, string email)
        {
            // 🔴 Validación HU
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // 🔴 Duplicado
            if (usuarios.Any(u => u.email == email))
            {
                ViewBag.Error = "Usuario duplicado";
                return View();
            }

            usuarios.Add(new { id = usuarios.Count + 1, nombre, email });

            return RedirectToAction("Index");
        }

        // ELIMINAR
        public IActionResult Eliminar(int id)
        {
            var user = usuarios.FirstOrDefault(u => u.id == id);

            if (user != null)
            {
                usuarios.Remove(user);
            }

            return RedirectToAction("Index");
        }
    }
}