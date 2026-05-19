using Microsoft.AspNetCore.Mvc;
using WebApplicationAPP.Models;
using System.Linq;

namespace WebApplicationAPP.Controllers
{
    public class SeguridadController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public SeguridadController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // GET
        public IActionResult Index()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            // Validar campos vacíos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";
                return View();
            }

            // Buscar usuario en la BD
            var user = _context.Usuarios
                .FirstOrDefault(u =>
                    u.Username == usuario &&
                    u.PasswordHash == password);

            // Si existe
            if (user != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // Error login
            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Seguridad");
        }
    }
}