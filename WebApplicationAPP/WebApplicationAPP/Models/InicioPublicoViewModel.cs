
using WebApplicationAPP.Models;

namespace WebApplicationAPP.Models
{
    public class InicioPublicoViewModel
    {
        public List<Servicio> Servicios { get; set; } = new();

        public List<string> ImagenesGaleria { get; set; } = new();

        public Contacto? Contacto { get; set; }
    }
}
