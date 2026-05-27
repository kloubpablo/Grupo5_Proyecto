using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public UsuariosController(YampiBarbershopContext context)
        {
            _context = context;
        }

        // VALIDAR ADMIN
        private bool EsAdmin()
        {
            return HttpContext.Session
                .GetString("Rol") == "Administrador";
        }

        // LISTA
        public IActionResult Index()
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .ToList();

            return View(usuarios);
        }

        // CREAR GET
        public IActionResult Crear()
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            ViewBag.Roles = _context.Roles
                .Where(r => r.Estado == true)
                .ToList();

            return View();
        }

        // CREAR POST
        [HttpPost]
        public IActionResult Crear(
            string nombre,
            string username,
            string correoElectronico,
            string password,
            string confirmarContraseña,
            int idRol)
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            // VALIDAR CAMPOS
            if (string.IsNullOrEmpty(nombre) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(correoElectronico) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error =
                    "Debe completar todos los campos";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado == true)
                    .ToList();

                return View();
            }

            // VALIDAR CONTRASEÑAS
            if (password != confirmarContraseña)
            {
                ViewBag.Error =
                    "Las contraseñas deben coincidir";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado == true)
                    .ToList();

                return View();
            }

            // VALIDAR USUARIO
            bool existeUsuario = _context.Usuarios
                .Any(u => u.Username == username);

            if (existeUsuario)
            {
                ViewBag.Error =
                    "El usuario ya existe";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado == true)
                    .ToList();

                return View();
            }

            // VALIDAR CORREO
            bool existeCorreo = _context.Usuarios
                .Any(u =>
                    u.CorreoElectronico ==
                    correoElectronico);

            if (existeCorreo)
            {
                ViewBag.Error =
                    "El correo ya está registrado";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado == true)
                    .ToList();

                return View();
            }

            // CREAR USUARIO
            var usuario = new Usuario
            {
                Nombre = nombre,
                Username = username,
                CorreoElectronico = correoElectronico,
                PasswordHash = password,
                IdRol = idRol,
                ContraTemp = false
            };

            _context.Usuarios.Add(usuario);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ELIMINAR
        public IActionResult Eliminar(int id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}