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

            // Convertimos la ruta del controlador al nombre del privilegio
            string nombrePrivilegio = permiso switch
            {
                var p when p.StartsWith("Clientes") => "Gestionar Clientes",
                var p when p.StartsWith("Citas") => "Gestionar Citas",
                var p when p.StartsWith("Pagos") => "Gestionar Pagos",
                var p when p.StartsWith("Roles") => "Gestionar Roles",
                var p when p.StartsWith("Usuarios") => "Gestionar Usuarios",
                var p when p.StartsWith("Atencion") => "Gestionar Atención",
                var p when p.StartsWith("Reportes") => "Ver Reportes",
                _ => permiso
            };

            var rol = db.Roles
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.Nombre == nombreRol);

            if (rol == null)
                return false;

            return rol.IdPrivilegios.Any(p => p.Nombre == nombrePrivilegio);
        }
    }
}