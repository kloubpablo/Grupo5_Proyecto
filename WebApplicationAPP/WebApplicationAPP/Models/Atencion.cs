using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

public partial class Atencion
{
    public int IdAtencion { get; set; }

    public int IdCliente { get; set; }

    public int? IdCita { get; set; }

    public string Estado { get; set; } = null!;

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public virtual Cita? IdCitaNavigation { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
