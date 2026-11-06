using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplicationAPP.Helpers
{
    /// <summary>
    /// Catalogo de acciones compatibles con los tipos de cuenta soportados.
    /// Esta lista NO concede permisos: solo determina lo que un Administrador
    /// puede ofrecer en Roles. La asignacion real vive en rolesprivilegios.
    /// Las reglas de propiedad (citas, atenciones y pagos propios) se aplican
    /// obligatoriamente en el controlador y no pueden delegarse.
    /// </summary>
    public static class PoliticaPermisos
    {
        private static readonly HashSet<string> Cliente = new(StringComparer.OrdinalIgnoreCase)
        {
            "Citas/Crear", "Citas/MisCitas", "Citas/HorariosDisponibles",
            "Citas/Editar", "Citas/Cancelar"
        };

        private static readonly HashSet<string> Barbero = new(StringComparer.OrdinalIgnoreCase)
        {
            "Dashboard/Index",
            "Citas/Crear", "Citas/MisCitas", "Citas/HorariosDisponibles",
            "Citas/Editar", "Citas/Cancelar",
            "Atencion/Index", "Atencion/Agregar", "Atencion/RegistrarDesdeCita",
            "Atencion/Iniciar", "Atencion/Cancelar", "Atencion/Historial",
            "Pagos/Index", "Pagos/Crear",
            "Clientes/Index", "Clientes/Crear", "Clientes/Editar"
        };

        private static readonly Dictionary<string, string[]> Requisitos = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Citas/Editar"] = new[] { "Citas/MisCitas" },
            ["Citas/Cancelar"] = new[] { "Citas/MisCitas" },
            ["Atencion/Agregar"] = new[] { "Atencion/Index" },
            ["Atencion/RegistrarDesdeCita"] = new[] { "Atencion/Index" },
            ["Atencion/Iniciar"] = new[] { "Atencion/Index" },
            ["Atencion/Cancelar"] = new[] { "Atencion/Index" },
            ["Atencion/Historial"] = new[] { "Atencion/Index" },
            ["Pagos/Crear"] = new[] { "Atencion/Index" },
            ["Clientes/Crear"] = new[] { "Clientes/Index" },
            ["Clientes/Editar"] = new[] { "Clientes/Index" }
        };

        public static bool EsAdministrador(string? rol) =>
            string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase);

        public static bool EsTipoConfigurado(string? rol) =>
            string.Equals(rol, "Barbero", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rol, "Cliente", StringComparison.OrdinalIgnoreCase);

        public static bool PuedeAsignar(string? rol, string? permiso)
        {
            if (string.IsNullOrWhiteSpace(permiso)) return false;
            if (string.Equals(rol, "Barbero", StringComparison.OrdinalIgnoreCase))
                return Barbero.Contains(permiso);
            if (string.Equals(rol, "Cliente", StringComparison.OrdinalIgnoreCase))
                return Cliente.Contains(permiso);
            return false;
        }

        public static IReadOnlyList<string> DependenciasDe(string permiso) =>
            Requisitos.TryGetValue(permiso, out var requisitos)
                ? requisitos : Array.Empty<string>();

        public static bool DependenciasSatisfechas(ISet<string> activos, string permiso) =>
            DependenciasDe(permiso).All(activos.Contains);

        public static bool EstaHabilitado(ISet<string> activos, string permiso) =>
            activos.Contains(permiso) && DependenciasSatisfechas(activos, permiso);

        public static IReadOnlyList<string> DependientesActivos(ISet<string> activos, string permiso) =>
            activos.Where(p => DependenciasDe(p).Contains(permiso, StringComparer.OrdinalIgnoreCase))
                .OrderBy(p => p).ToList();
    }
}
