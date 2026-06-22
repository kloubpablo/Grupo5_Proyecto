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

        private bool EsAdmin()
        {
            return HttpContext.Session.GetString("Rol")
                == "Administrador";
        }

       
        // LISTA
     
        public IActionResult Index()
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .ToList();

            return View(usuarios);
        }

        public IActionResult Editar(int id)
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == id);

            if (usuario == null)
                return RedirectToAction(nameof(Index));

            ViewBag.Roles = _context.Roles
                .Where(r => r.Estado)
                .ToList();

            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(
            int idUsuario,
            string nombre,
            string username,
            string correoElectronico,
            int idRol,
            bool estado,
            string? password,
            string? confirmarContraseña)
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null)
                return RedirectToAction(nameof(Index));

            if (_context.Usuarios.Any(u =>
                u.Username == username &&
                u.IdUsuario != idUsuario))
            {
                ViewBag.Error = "El usuario ya existe";
                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado)
                    .ToList();

                return View(usuario);
            }

            if (_context.Usuarios.Any(u =>
                u.CorreoElectronico == correoElectronico &&
                u.IdUsuario != idUsuario))
            {
                ViewBag.Error = "El correo ya existe";
                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado)
                    .ToList();

                return View(usuario);
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password != confirmarContraseña)
                {
                    ViewBag.Error =
                        "Las contraseñas no coinciden";

                    ViewBag.Roles = _context.Roles
                        .Where(r => r.Estado)
                        .ToList();

                    return View(usuario);
                }

                usuario.PasswordHash = password;
            }

            usuario.Nombre = nombre;
            usuario.Username = username;
            usuario.CorreoElectronico = correoElectronico;
            usuario.IdRol = idRol;
            usuario.Estado = estado;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Perfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return RedirectToAction("Login", "Auth");

            var usuario = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            return View(usuario);
        }

        [HttpPost]
        public IActionResult Perfil(
            string nombre,
            string username,
            string correoElectronico,
            string? password,
            string? confirmarContraseña)
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return RedirectToAction("Login", "Auth");

            var usuario = _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefault(u =>
                u.IdUsuario == idUsuario);

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            if (_context.Usuarios.Any(u =>
                u.Username == username &&
                u.IdUsuario != usuario.IdUsuario))
            {
                ViewBag.Error = "El nombre de usuario ya existe";
                return View(usuario);
            }

            if (_context.Usuarios.Any(u =>
                u.CorreoElectronico == correoElectronico &&
                u.IdUsuario != usuario.IdUsuario))
            {
                ViewBag.Error = "El correo ya existe";
                return View(usuario);
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password != confirmarContraseña)
                {
                    ViewBag.Error =
                        "Las contraseñas no coinciden";

                    return View(usuario);
                }

                if (password.Length < 8)
                {
                    ViewBag.Error =
                        "La contraseña debe tener al menos 8 caracteres";

                    return View(usuario);
                }

                usuario.PasswordHash = password;
            }

            

            usuario.Nombre = nombre;
            usuario.Username = username;
            usuario.CorreoElectronico = correoElectronico;

            _context.SaveChanges();

            ViewBag.Exito = "Perfil actualizado correctamente";

            return View(usuario);
        }

        // CREAR
      
        public IActionResult Crear()
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            ViewBag.Roles = _context.Roles
                .Where(r => r.Estado)
                .ToList();

            return View();
        }

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
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            if (string.IsNullOrEmpty(nombre) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(correoElectronico) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error =
                    "Debe completar todos los campos";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado)
                    .ToList();

                return View();
            }

            if (password != confirmarContraseña)
            {
                ViewBag.Error =
                    "Las contraseñas no coinciden";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado)
                    .ToList();

                return View();
            }

            if (_context.Usuarios.Any(u =>
                u.Username == username))
            {
                ViewBag.Error =
                    "El usuario ya existe";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado)
                    .ToList();

                return View();
            }

            if (_context.Usuarios.Any(u =>
                u.CorreoElectronico ==
                correoElectronico))
            {
                ViewBag.Error =
                    "El correo ya existe";

                ViewBag.Roles = _context.Roles
                    .Where(r => r.Estado)
                    .ToList();

                return View();
            }

            Usuario usuario = new Usuario();

            usuario.Nombre = nombre;
            usuario.Username = username;
            usuario.CorreoElectronico =
                correoElectronico;
            usuario.PasswordHash = password;
            usuario.IdRol = idRol;
            usuario.ContraTemp = false;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        
        // CAMBIAR ROL
      
        public IActionResult CambiarRol(int id)
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            var usuario = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u =>
                    u.IdUsuario == id);

            if (usuario == null)
                return RedirectToAction(nameof(Index));

            ViewBag.Usuario = usuario;

            ViewBag.Roles = _context.Roles
                .Where(r => r.Estado)
                .ToList();


            return View();
        }

        [HttpPost]
        public IActionResult CambiarRol(
        int idUsuario,
        int idRol,
        bool estado)
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == idUsuario);

            if (usuario == null)
                return RedirectToAction(nameof(Index));

            usuario.IdRol = idRol;
            usuario.Estado = estado;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR

        public IActionResult Eliminar(int id)
        {
            if (!EsAdmin())
                return RedirectToAction(
                    "Index",
                    "Dashboard");

            var usuario = _context.Usuarios
                .FirstOrDefault(u =>
                    u.IdUsuario == id);

            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}