using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class VwPagosRealizado
{
    public int IdPago { get; set; }

    public string Cliente { get; set; } = null!;

    public decimal Monto { get; set; }

    public string Metodo { get; set; } = null!;

    public DateOnly Fecha { get; set; }
}
