
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;
using WebApplicationAPP.Helpers;

namespace WebApplicationAPP.Controllers
{
    public class RolesController : Controller
    {
        private readonly YampiBarbershopContext _context;

        public RolesController(YampiBarbershopContext context)
        {
            _context = context;
        }

        private bool TienePermiso(string permiso)
        {
            var rol = HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(rol))
                return false;

            return PermisosHelper.TienePermiso(_context, rol, permiso);
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

        // ==========================================
        // LISTADO
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            if (!TienePermiso("Roles/Index"))
                return RedirectToAction("Index", "Dashboard");

            var roles = _context.Roles
                .OrderByDescending(r => r.IdRol)
                .ToList();

            return View(roles);
        }

        // ==========================================
        // CREAR - GET
        // ==========================================

        [HttpGet]
        public IActionResult Crear()
        {
            if (!TienePermiso("Roles/Crear"))
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CREAR - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(string nombre, string? descripcion)
        {
            if (!TienePermiso("Roles/Crear"))
                return SinPermiso();

            nombre = nombre?.Trim() ?? "";
            descripcion = descripcion?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre))
                return Error("Debe ingresar el nombre del rol");

            bool existe = _context.Roles.Any(r =>
                r.Nombre == nombre);

            if (existe)
                return Error("El rol ya existe");

            var rol = new Role
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Estado = true
            };

            _context.Roles.Add(rol);
            _context.SaveChanges();

            return Json(new
            {
                ok = true,
                mensaje = "Rol creado correctamente",
                rol = new
                {
                    idRol = rol.IdRol,
                    nombre = rol.Nombre,
                    descripcion = rol.Descripcion,
                    estado = rol.Estado
                }
            });
        }

        // ==========================================
        // CAMBIAR ESTADO - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id)
        {
            if (!TienePermiso("Roles/Index"))
                return SinPermiso();

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == id);

            if (rol == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Rol no encontrado"
                });

            rol.Estado = !rol.Estado;

            _context.SaveChanges();

            return Json(new
            {
                ok = true,
                mensaje = rol.Estado
                    ? "Rol activado correctamente"
                    : "Rol desactivado correctamente",
                estado = rol.Estado,
                idRol = rol.IdRol
            });
        }

        // ==========================================
        // ELIMINAR - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (!TienePermiso("Roles/Index"))
                return SinPermiso();

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == id);

            if (rol == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Rol no encontrado"
                });

            try
            {
                _context.Roles.Remove(rol);
                _context.SaveChanges();

                return Json(new
                {
                    ok = true,
                    mensaje = "Rol eliminado correctamente",
                    idRol = id
                });
            }
            catch (DbUpdateException)
            {
                return Error(
                    "No se puede eliminar el rol porque tiene registros relacionados"
                );
            }
        }

        // ==========================================
        // CONSULTAR PERMISOS - GET JSON
        // ==========================================

        [HttpGet]
        public IActionResult Permisos(int id)
        {
            if (!TienePermiso("Roles/Permisos"))
                return SinPermiso();

            var rol = _context.Roles
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.IdRol == id);

            if (rol == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Rol no encontrado"
                });

            var permisos = _context.Privilegios
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToList()
                .Select(p => new
                {
                    idPrivilegio = p.IdPrivilegio,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion ?? "",
                    seleccionado = rol.IdPrivilegios.Any(x =>
                        x.IdPrivilegio == p.IdPrivilegio)
                })
                .ToList();

            return Json(new
            {
                ok = true,
                idRol = rol.IdRol,
                nombreRol = rol.Nombre,
                permisos
            });
        }

        // ==========================================
        // ACTIVAR / DESACTIVAR PERMISO - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TogglePermiso(
            int idRol,
            int idPrivilegio)
        {
            if (!TienePermiso("Roles/Permisos"))
                return SinPermiso();

            var rol = _context.Roles
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.IdRol == idRol);

            if (rol == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Rol no encontrado"
                });

            var permiso = _context.Privilegios
                .FirstOrDefault(p =>
                    p.IdPrivilegio == idPrivilegio);

            if (permiso == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Permiso no encontrado"
                });

            bool seleccionado;

            if (rol.IdPrivilegios.Any(p =>
                p.IdPrivilegio == idPrivilegio))
            {
                rol.IdPrivilegios.Remove(permiso);
                seleccionado = false;
            }
            else
            {
                rol.IdPrivilegios.Add(permiso);
                seleccionado = true;
            }

            _context.SaveChanges();

            return Json(new
            {
                ok = true,
                mensaje = seleccionado
                    ? "Permiso activado correctamente"
                    : "Permiso desactivado correctamente",
                idRol,
                idPrivilegio,
                seleccionado
            });
        }
    }
}
