using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class SeguridadController : Controller
    {
        // GET: Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            // 🔴 Validación de campos vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // 🟢 Validación de credenciales
            if (usuario == "admin" && password == "123")
            {
                // 👉 Redirige al Dashboard
                return RedirectToAction("Index", "Dashboard");
            }

            // 🔴 Error de login
            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            // (Luego podemos agregar sesión aquí)
            return RedirectToAction("Index", "Seguridad");
        }
    }
}