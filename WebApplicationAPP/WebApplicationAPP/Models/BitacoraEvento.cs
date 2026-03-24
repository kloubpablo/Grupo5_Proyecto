using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplicationAPP.Models
{
    [Table("BITACORA_EVENTOS_G4")]
    public class BitacoraEvento
    {
        [Key]
        public int IdEvento { get; set; }

        [Required]
        [StringLength(20)]
        public string TablaDeEvento { get; set; }

        [Required]
        [StringLength(20)]
        public string TipoDeEvento { get; set; }

        public DateTime FechaDeEvento { get; set; }

        [Required]
        public string DescripcionDeEvento { get; set; }

        [Required]
        public string StackTrace { get; set; }

        public string? DatosAnteriores { get; set; }

        public string? DatosPosteriores { get; set; }
    }
}
