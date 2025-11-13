using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities
{
    public class ProcesoIA
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentoId { get; set; }

        [Required, MaxLength(100)]
        public string TipoProcesamiento { get; set; } = "analisis_documento";

        public DateTime FechaInicio { get; set; } = DateTime.UtcNow;
        
        public DateTime? FechaFin { get; set; }

        [Required, MaxLength(50)]
        public string Estado { get; set; } = "pendiente"; // "pendiente", "procesando", "completado", "error"

        [Column(TypeName = "nvarchar(max)")]
        public string? ResultadoJson { get; set; }

        [MaxLength(1000)]
        public string? Error { get; set; }

        [MaxLength(500)]
        public string? UrlServicio { get; set; }

        public double? TiempoProcesamientoSegundos { get; set; }

        // Relación
        [ForeignKey("DocumentoId")]
        public virtual Documento Documento { get; set; } = null!;
    }
}