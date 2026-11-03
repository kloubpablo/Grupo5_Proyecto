using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Helpers
{
    public static class PermisosHelper
    {
        public static bool TienePermiso(
            YampiBarbershopContext db,
            string nombreRol,
            string permiso)
        {
            if (string.IsNullOrEmpty(nombreRol))
                return false;

            // El administrador tiene acceso total
            if (nombreRol == "Administrador")
                return true;

            var rol = db.Roles
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.Nombre == nombreRol);

            if (rol == null)
                return false;

            // Comprueba los privilegios
            return rol.IdPrivilegios.Any(p =>
                p.Nombre == permiso);
        }
    }
}