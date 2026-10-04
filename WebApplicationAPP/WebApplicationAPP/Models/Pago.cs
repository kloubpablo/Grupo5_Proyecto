using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int IdCliente { get; set; }

    public decimal Monto { get; set; }

    public string Metodo { get; set; } = null!;

    public DateOnly Fecha { get; set; }

    public bool Cerrado { get; set; }
    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
