using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WebApplicationAPP.Models
{
    [Table("Cajas_G4")]
    
    public class Cajas
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Comercio")]
        public int IdComercio { get; set; }
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string TelefonoSINPE { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        public DateTime? FechaDeModificacion { get; set; }

        public bool Estado { get; set; } = true;

        public virtual Comercio Comercio { get; set; }

    }
}
