using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Atencion> Atencions { get; set; } = new List<Atencion>();

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
