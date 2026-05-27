using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Role
{
    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Estado { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

    public virtual ICollection<Privilegio> IdPrivilegios { get; set; } = new List<Privilegio>();
}
