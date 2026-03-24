using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace WebApplicationAPP.Models
{
    [Table("SINPE_G4")]
    public class Sinpe
    {
        [Key]
        public int IdSinpe { get; set; }

        [Required]
        [StringLength(10)]
        public string TelefonoOrigen { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreOrigen { get; set; }

        [Required]
        [StringLength(10)]
        public string TelefonoDestinatario { get; set; }

        

        [Required]
        [StringLength(200)]
        public string NombreDestinatario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public DateTime FechaDeRegistro { get; set; }

        [StringLength(50)]
        public string? Descripcion { get; set; }

        public bool Estado { get; set; } = false; // 0 = No sincronizado
    }
}
