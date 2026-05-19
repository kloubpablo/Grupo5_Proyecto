using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class VwEstadoAtencion
{
    public string Nombre { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }
}
