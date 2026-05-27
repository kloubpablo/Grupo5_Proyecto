using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Privilegio
{
    public int IdPrivilegio { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public virtual ICollection<Role> IdRols { get; set; } = new List<Role>();
}
