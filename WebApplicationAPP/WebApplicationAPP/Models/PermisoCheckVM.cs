using System;
using System.Collections.Generic;


namespace WebApplicationAPP.Models
{
    public class PermisoCheckVM
    {
        public int IdPrivilegio { get; set; }

        public string Nombre { get; set; } = "";

        public string Descripcion { get; set; } = "";

        public bool Seleccionado { get; set; }
    }
}