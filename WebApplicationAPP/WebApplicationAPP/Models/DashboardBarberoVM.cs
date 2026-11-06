using System;
using System.Collections.Generic;

namespace WebApplicationAPP.Models;

// Solo datos del barbero autenticado; nunca incluye totales de otros empleados.
public sealed class DashboardBarberoVM
{
    public string NombreBarbero { get; set; } = "";
    public DateOnly Fecha { get; set; }
    public int CitasHoy { get; set; }
    public int CitasActivasHoy { get; set; }
    public int CitasCanceladasHoy { get; set; }
    public int CitasProximas { get; set; }
    public int AtencionesEnEspera { get; set; }
    public int AtencionesEnServicio { get; set; }
    public int AtencionesFinalizadas { get; set; }
    public int CitasCanceladasMes { get; set; }
    public int PagosHoy { get; set; }
    public decimal IngresosHoy { get; set; }
    public decimal IngresosMes { get; set; }
    public List<Cita> AgendaHoy { get; set; } = new();
    public List<string> DiasGrafico { get; set; } = new();
    public List<int> CantidadesGrafico { get; set; } = new();
}
