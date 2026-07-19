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

        //LISTA DE ROLES
        public IActionResult Index()
        {
            if (!TienePermiso("Roles/Index"))
                return RedirectToAction("Index", "Dashboard");

            var roles = _context.Roles.ToList();
            return View(roles);
        }

        //CREAR (GET)
        public IActionResult Crear()
        {
            if (!TienePermiso("Roles/Crear"))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        //CREAR (POST)
        [HttpPost]
        public IActionResult Crear(string nombre, string descripcion)
        {
            if (!TienePermiso("Roles/Crear"))
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrWhiteSpace(nombre))
            {
                ViewBag.Error = "Debe ingresar el nombre del rol";
                return View();
            }

            bool existe = _context.Roles.Any(r => r.Nombre == nombre);

            if (existe)
            {
                ViewBag.Error = "El rol ya existe";
                return View();
            }

            var rol = new Role
            {
                Nombre = nombre,
                Descripcion = descripcion,
                Estado = true
            };

            _context.Roles.Add(rol);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        //ACTIVAR / DESACTIVAR
        public IActionResult Toggle(int id)
        {
            if (!TienePermiso("Roles/Index"))
                return RedirectToAction("Index", "Dashboard");

            var rol = _context.Roles.FirstOrDefault(r => r.IdRol == id);

            if (rol != null)
            {
                rol.Estado = !rol.Estado;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        //ELIMINAR
        public IActionResult Eliminar(int id)
        {
            if (!TienePermiso("Roles/Index"))
                return RedirectToAction("Index", "Dashboard");

            var rol = _context.Roles.FirstOrDefault(r => r.IdRol == id);

            if (rol != null)
            {
                _context.Roles.Remove(rol);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        //PERMISOS (GET)
        public IActionResult Permisos(int id)
        {
            if (!TienePermiso("Roles/Permisos"))
                return RedirectToAction("Index", "Dashboard");

            var rol = _context.Roles
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.IdRol == id);

            if (rol == null)
                return RedirectToAction(nameof(Index));

            var vm = new PermisosRolVM
            {
                IdRol = rol.IdRol,
                NombreRol = rol.Nombre,

                Permisos = _context.Privilegios
                    .ToList()
                    .Select(p => new PermisoCheckVM
                    {
                        IdPrivilegio = p.IdPrivilegio,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion ?? "",
                        Seleccionado = rol.IdPrivilegios
                            .Any(x => x.IdPrivilegio == p.IdPrivilegio)
                    })
                    .ToList()
            };

            return View(vm);
        }

        //TOGGLE PERMISO
        [HttpPost]
        public IActionResult TogglePermiso(int idRol, int idPrivilegio)
        {
            if (!TienePermiso("Roles/Permisos"))
                return RedirectToAction("Index", "Dashboard");

            var rol = _context.Roles
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.IdRol == idRol);

            if (rol == null)
                return RedirectToAction("Index");

            var permiso = _context.Privilegios
                .FirstOrDefault(p => p.IdPrivilegio == idPrivilegio);

            if (permiso == null)
                return RedirectToAction("Permisos", new { id = idRol });

            if (rol.IdPrivilegios.Any(p => p.IdPrivilegio == idPrivilegio))
                rol.IdPrivilegios.Remove(permiso);
            else
                rol.IdPrivilegios.Add(permiso);

            _context.SaveChanges();

            return RedirectToAction("Permisos", new { id = idRol });
        }
    }
}