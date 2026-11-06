using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Helpers
{
    public static class PermisosHelper
    {
        // Contrasta los permisos de MariaDB con la politica compatible.
        // Las asignaciones antiguas ajenas al rol no generan acceso efectivo.
        public static HashSet<string> ObtenerPermisos(YampiBarbershopContext db, Usuario usuario)
        {
            var vacio = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (usuario == null || !usuario.Estado || usuario.ContraTemp) return vacio;

            var rol = db.Roles.AsNoTracking()
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.IdRol == usuario.IdRol && r.Estado);
            if (rol == null || PoliticaPermisos.EsAdministrador(rol.Nombre)) return vacio;

            var asignados = new HashSet<string>(
                rol.IdPrivilegios.Select(p => p.Nombre), StringComparer.OrdinalIgnoreCase);
            return asignados.Where(p => PoliticaPermisos.PuedeAsignar(rol.Nombre, p) &&
                    PoliticaPermisos.DependenciasSatisfechas(asignados, p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        // Preferir esta sobrecarga: verifica que el usuario este activo en BD.
        public static bool TienePermiso(YampiBarbershopContext db, Usuario? usuario, string permiso)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(permiso) ||
                !usuario.Estado || usuario.ContraTemp) return false;

            var rol = db.Roles.AsNoTracking()
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.IdRol == usuario.IdRol && r.Estado);
            if (rol == null) return false;
            if (PoliticaPermisos.EsAdministrador(rol.Nombre)) return true;
            if (!PoliticaPermisos.PuedeAsignar(rol.Nombre, permiso)) return false;
            var asignados = rol.IdPrivilegios.Select(p => p.Nombre)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return PoliticaPermisos.EstaHabilitado(asignados, permiso);
        }

        // Compatibilidad con vistas antiguas. NO verifica la identidad del usuario;
        // no utilizar para autorizar solicitudes mutables: preferir Usuario.
        public static bool TienePermiso(YampiBarbershopContext db, string? nombreRol, string permiso)
        {
            if (string.IsNullOrWhiteSpace(nombreRol) || string.IsNullOrWhiteSpace(permiso))
                return false;

            var rol = db.Roles.AsNoTracking()
                .Include(r => r.IdPrivilegios)
                .FirstOrDefault(r => r.Nombre == nombreRol && r.Estado);
            if (rol == null) return false;
            if (PoliticaPermisos.EsAdministrador(rol.Nombre)) return true;
            if (!PoliticaPermisos.PuedeAsignar(rol.Nombre, permiso)) return false;

            var activos = rol.IdPrivilegios.Select(p => p.Nombre)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return PoliticaPermisos.EstaHabilitado(activos, permiso);
        }
    }
}
