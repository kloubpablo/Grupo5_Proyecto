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

        // LISTA DE USUARIOS
        public IActionResult Index()
        {
            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .ToList();

            return View(usuarios);
        }

        // CREAR (GET)
        public IActionResult Crear()
        {
            ViewBag.Roles = _context.Roles.ToList();

            return View();
        }

        // CREAR (POST)
        [HttpPost]
        public IActionResult Crear(
            string nombre,
            string username,
            string password,
            int idRol)
        {
            // VALIDAR CAMPOS
            if (string.IsNullOrEmpty(nombre) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe completar todos los campos";
                ViewBag.Roles = _context.Roles.ToList();

                return View();
            }

            // VALIDAR DUPLICADO
            bool existe = _context.Usuarios
                .Any(u => u.Username == username);

            if (existe)
            {
                ViewBag.Error = "El usuario ya existe";
                ViewBag.Roles = _context.Roles.ToList();

                return View();
            }

            // CREAR USUARIO
            var usuario = new Usuario
            {
                Nombre = nombre,
                Username = username,
                PasswordHash = password,
                IdRol = idRol
            };

            _context.Usuarios.Add(usuario);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ELIMINAR
        public IActionResult Eliminar(int id)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}