using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class BitacoraCliente
{
    public int IdBitacora { get; set; }

    public int? IdCliente { get; set; }

    public string? Accion { get; set; }

    public DateTime? Fecha { get; set; }
}
