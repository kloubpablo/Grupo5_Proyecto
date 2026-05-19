using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Reporte
{
    public int IdReporte { get; set; }

    public DateOnly Fecha { get; set; }

    public decimal IngresosTotales { get; set; }

    public DateTime? CreatedAt { get; set; }
}
