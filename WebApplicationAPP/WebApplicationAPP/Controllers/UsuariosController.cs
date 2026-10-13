
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

        // ==========================================
        // PERMISOS
        // ==========================================

        private bool EsAdmin()
        {
            return HttpContext.Session.GetString("Rol")
                == "Administrador";
        }

        private IActionResult SinPermiso()
        {
            return StatusCode(403, new
            {
                ok = false,
                mensaje = "No tienes permisos o tu sesion ha expirado"
            });
        }

        private IActionResult Error(string mensaje)
        {
            return BadRequest(new
            {
                ok = false,
                mensaje
            });
        }

        private object DatosUsuario(Usuario usuario, string rolNombre)
        {
            return new
            {
                idUsuario = usuario.IdUsuario,
                nombre = usuario.Nombre,
                username = usuario.Username,
                correoElectronico = usuario.CorreoElectronico,
                idRol = usuario.IdRol,
                rolNombre,
                estado = usuario.Estado
            };
        }

        // ==========================================
        // INDEX
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Dashboard");

            ViewBag.Roles = _context.Roles
                .Where(r => r.Estado)
                .OrderBy(r => r.Nombre)
                .ToList();

            var usuarios = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .OrderByDescending(u => u.IdUsuario)
                .ToList();

            return View(usuarios);
        }

        // ==========================================
        // CREAR - GET
        // ==========================================

        [HttpGet]
        public IActionResult Crear()
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CREAR - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            string nombre,
            string username,
            string correoElectronico,
            string password,
            string confirmarContraseña,
            int idRol)
        {
            if (!EsAdmin())
                return SinPermiso();

            nombre = nombre?.Trim() ?? "";
            username = username?.Trim() ?? "";
            correoElectronico = correoElectronico?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(correoElectronico) ||
                string.IsNullOrWhiteSpace(password))
            {
                return Error("Debes completar todos los campos");
            }

            if (password.Length < 8)
                return Error("La contraseña debe tener al menos 8 caracteres");

            if (password != confirmarContraseña)
                return Error("Las contraseñas no coinciden");

            if (_context.Usuarios.Any(u => u.Username == username))
                return Error("El nombre de usuario ya existe");

            if (_context.Usuarios.Any(u =>
                u.CorreoElectronico == correoElectronico))
            {
                return Error("El correo electronico ya existe");
            }

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == idRol && r.Estado);

            if (rol == null)
                return Error("Selecciona un rol valido");

            var usuario = new Usuario
            {
                Nombre = nombre,
                Username = username,
                CorreoElectronico = correoElectronico,

                // Compatible con la autenticacion actual.
                // Pendiente migrar a hash seguro en todo el proyecto.
                PasswordHash = password,

                IdRol = idRol,
                ContraTemp = false,
                Estado = true
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Json(new
            {
                ok = true,
                mensaje = "Usuario creado correctamente",
                usuario = DatosUsuario(usuario, rol.Nombre)
            });
        }

        // ==========================================
        // EDITAR - GET
        // ==========================================

        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // EDITAR - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                return SinPermiso();

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Usuario no encontrado"
                });

            nombre = nombre?.Trim() ?? "";
            username = username?.Trim() ?? "";
            correoElectronico = correoElectronico?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(correoElectronico))
            {
                return Error("Debes completar todos los campos");
            }

            if (_context.Usuarios.Any(u =>
                u.Username == username &&
                u.IdUsuario != idUsuario))
            {
                return Error("El nombre de usuario ya existe");
            }

            if (_context.Usuarios.Any(u =>
                u.CorreoElectronico == correoElectronico &&
                u.IdUsuario != idUsuario))
            {
                return Error("El correo electronico ya existe");
            }

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == idRol && r.Estado);

            if (rol == null)
                return Error("Selecciona un rol valido");

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 8)
                    return Error(
                        "La contraseña debe tener al menos 8 caracteres"
                    );

                if (password != confirmarContraseña)
                    return Error("Las contraseñas no coinciden");

                // Compatible con el sistema actual.
                usuario.PasswordHash = password;
            }

            usuario.Nombre = nombre;
            usuario.Username = username;
            usuario.CorreoElectronico = correoElectronico;
            usuario.IdRol = idRol;
            usuario.Estado = estado;

            _context.SaveChanges();

            return Json(new
            {
                ok = true,
                mensaje = "Usuario actualizado correctamente",
                usuario = DatosUsuario(usuario, rol.Nombre)
            });
        }

        // ==========================================
        // CAMBIAR ROL - GET
        // ==========================================

        [HttpGet]
        public IActionResult CambiarRol(int id)
        {
            if (!EsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CAMBIAR ROL - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarRol(
            int idUsuario,
            int idRol,
            bool estado)
        {
            if (!EsAdmin())
                return SinPermiso();

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Usuario no encontrado"
                });

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == idRol && r.Estado);

            if (rol == null)
                return Error("El rol seleccionado no es valido");

            usuario.IdRol = idRol;
            usuario.Estado = estado;

            _context.SaveChanges();

            return Json(new
            {
                ok = true,
                mensaje = "Rol y estado actualizados correctamente",
                usuario = DatosUsuario(usuario, rol.Nombre)
            });
        }

        // ==========================================
        // ELIMINAR - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (!EsAdmin())
                return SinPermiso();

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == id);

            if (usuario == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Usuario no encontrado"
                });

            int? idActual = HttpContext.Session.GetInt32("IdUsuario");

            if (idActual == usuario.IdUsuario)
                return Error("No puedes eliminar tu propia cuenta");

            try
            {
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();

                return Json(new
                {
                    ok = true,
                    mensaje = "Usuario eliminado correctamente"
                });
            }
            catch (DbUpdateException)
            {
                return Error(
                    "No se puede eliminar porque el usuario tiene registros relacionados"
                );
            }
        }

        // ==========================================
        // PERFIL - GET
        // ==========================================

        [HttpGet]
        public IActionResult Perfil()
        {
            int? idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
                return RedirectToAction("Login", "Auth");

            var usuario = _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefault(u => u.IdUsuario == idUsuario.Value);

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            return View(usuario);
        }

        // ==========================================
        // PERFIL - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                .FirstOrDefault(u => u.IdUsuario == idUsuario.Value);

            if (usuario == null)
                return RedirectToAction("Login", "Auth");

            nombre = nombre?.Trim() ?? "";
            username = username?.Trim() ?? "";
            correoElectronico = correoElectronico?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(correoElectronico))
            {
                ViewBag.Error = "Debes completar todos los campos";
                return View(usuario);
            }

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
                ViewBag.Error = "El correo electronico ya existe";
                return View(usuario);
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                if (password.Length < 8)
                {
                    ViewBag.Error =
                        "La contraseña debe tener al menos 8 caracteres";
                    return View(usuario);
                }

                if (password != confirmarContraseña)
                {
                    ViewBag.Error = "Las contraseñas no coinciden";
                    return View(usuario);
                }

                // Compatible con el sistema actual.
                usuario.PasswordHash = password;
            }

            usuario.Nombre = nombre;
            usuario.Username = username;
            usuario.CorreoElectronico = correoElectronico;

            _context.SaveChanges();

            ViewBag.Exito = "Perfil actualizado correctamente";

            return View(usuario);
        }
    }
}
