using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class VwCitasDelDium
{
    public int IdCita { get; set; }

    public string Cliente { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    public string Barbero { get; set; } = null!;

    public string Estado { get; set; } = null!;
}
