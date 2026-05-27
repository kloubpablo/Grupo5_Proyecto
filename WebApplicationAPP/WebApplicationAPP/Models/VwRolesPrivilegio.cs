using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class VwRolesPrivilegio
{
    public string Rol { get; set; } = null!;

    public string Privilegio { get; set; } = null!;

    public string? Descripcion { get; set; }
}
