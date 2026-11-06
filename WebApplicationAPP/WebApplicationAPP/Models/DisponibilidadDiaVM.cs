namespace WebApplicationAPP.Models;
public sealed class DisponibilidadDiaVM
{
    public int IdBarbero { get; set; }
    public string Barbero { get; set; } = "";
    public List<string> HorasDisponibles { get; set; } = new();
}
