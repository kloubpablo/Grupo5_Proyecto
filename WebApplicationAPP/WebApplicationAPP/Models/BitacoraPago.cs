using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class BitacoraPago
{
    public int IdBitacora { get; set; }

    public int? IdPago { get; set; }

    public string? Accion { get; set; }

    public DateTime? Fecha { get; set; }
}
