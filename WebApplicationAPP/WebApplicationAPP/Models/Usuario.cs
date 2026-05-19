using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int IdRol { get; set; }

    public virtual Role IdRolNavigation { get; set; } = null!;
}
