using Microsoft.AspNetCore.Mvc;

namespace WebApplicationAPP.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string usuario, string password)
        {
            if (usuario == "admin" && password == "123")
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos";
            return View();
        }
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Login");
        }
    }
}
