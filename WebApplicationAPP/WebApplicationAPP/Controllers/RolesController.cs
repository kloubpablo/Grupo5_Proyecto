
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

        // Administrar permisos es una operacion exclusiva de Administrador.
        // No se delega mediante rolesprivilegios, para evitar autoasignaciones.
        private bool EsAdministradorActual()
        {
            int? id = HttpContext.Session.GetInt32("IdUsuario");
            return id.HasValue && _context.Usuarios.AsNoTracking()
                .Any(u => u.IdUsuario == id.Value && u.Estado && !u.ContraTemp &&
                    u.IdRolNavigation.Estado && u.IdRolNavigation.Nombre == "Administrador");
        }

        // La politica de compatibilidad se define en un solo archivo:
        // Helpers/PoliticaPermisos.cs. Nunca delegar operaciones de seguridad.

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
            if (!EsAdministradorActual())
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
            if (!EsAdministradorActual())
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
            if (!EsAdministradorActual()) return SinPermiso();
            return Error("Los roles personalizados requieren primero definir su tipo de cuenta y sus restricciones. Por seguridad, administra los perfiles existentes.");
        }

        // ==========================================
        // CAMBIAR ESTADO - POST AJAX
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id)
        {
            if (!EsAdministradorActual())
                return SinPermiso();

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == id);

            if (rol == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Rol no encontrado"
                });

            if (rol.Nombre == "Administrador")
                return Error("El rol Administrador esta protegido y no se puede modificar desde este modulo.");

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
            if (!EsAdministradorActual())
                return SinPermiso();

            var rol = _context.Roles
                .FirstOrDefault(r => r.IdRol == id);

            if (rol == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Rol no encontrado"
                });

            if (PoliticaPermisos.EsAdministrador(rol.Nombre) ||
                PoliticaPermisos.EsTipoConfigurado(rol.Nombre))
                return Error("Los roles principales no se pueden eliminar; modifica su estado o sus permisos.");

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
            if (!EsAdministradorActual())
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

            bool protegido = PoliticaPermisos.EsAdministrador(rol.Nombre);
            bool compatible = PoliticaPermisos.EsTipoConfigurado(rol.Nombre);
            var asignados = rol.IdPrivilegios.Select(p => p.IdPrivilegio).ToHashSet();
            var permisos = _context.Privilegios.AsNoTracking()
                .OrderBy(p => p.Nombre).ToList()
                // Unicamente se ofrecen acciones que tienen sentido para este tipo de cuenta.
                .Where(p => PoliticaPermisos.PuedeAsignar(rol.Nombre, p.Nombre))
                .Select(p => new
                {
                    idPrivilegio = p.IdPrivilegio,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion ?? "",
                    seleccionado = asignados.Contains(p.IdPrivilegio),
                    editable = true,
                    requiere = PoliticaPermisos.DependenciasDe(p.Nombre)
                }).ToList();

            return Json(new
            {
                ok = true,
                idRol = rol.IdRol,
                nombreRol = rol.Nombre,
                protegido,
                compatible,
                mensaje = protegido
                    ? "El Administrador conserva acceso global y no se configura desde esta pantalla."
                    : compatible
                        ? "Solo se muestran permisos compatibles con este rol."
                        : "Este tipo de rol no tiene una politica de acceso definida.",
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
            if (!EsAdministradorActual())
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

            if (rol.Nombre == "Administrador")
                return Error("El rol Administrador esta protegido y no se puede modificar desde este modulo.");

            var permiso = _context.Privilegios
                .FirstOrDefault(p =>
                    p.IdPrivilegio == idPrivilegio);

            if (permiso == null)
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Permiso no encontrado"
                });

            // La validacion real esta en servidor, no en la lista del navegador.
            if (!PoliticaPermisos.PuedeAsignar(rol.Nombre, permiso.Nombre))
                return Error("Este permiso no corresponde al rol seleccionado.");

            bool yaAsignado = rol.IdPrivilegios.Any(p => p.IdPrivilegio == idPrivilegio);
            var activos = rol.IdPrivilegios.Select(p => p.Nombre)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!yaAsignado)
            {
                var faltantes = PoliticaPermisos.DependenciasDe(permiso.Nombre)
                    .Where(req => !activos.Contains(req)).ToList();
                if (faltantes.Count > 0)
                    return Error("Activa primero: " + string.Join(", ", faltantes));
            }
            else
            {
                var dependientes = PoliticaPermisos.DependientesActivos(activos, permiso.Nombre);
                if (dependientes.Count > 0)
                    return Error("Desactiva primero: " + string.Join(", ", dependientes));
            }

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
