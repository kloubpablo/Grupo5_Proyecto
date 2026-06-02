using System;
using System.Collections.Generic;


namespace WebApplicationAPP.Models
{
    public class PermisosRolVM
    {
        public int IdRol { get; set; }

        public string NombreRol { get; set; } = "";

        public List<PermisoCheckVM> Permisos { get; set; }
            = new List<PermisoCheckVM>();
    }
}