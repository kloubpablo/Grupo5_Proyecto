using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Cita
{
    public int IdCita { get; set; }

    public int IdCliente { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    public string Barbero { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public virtual ICollection<Atencion> Atencions { get; set; } = new List<Atencion>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
