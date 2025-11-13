using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities
{
    public class Insight
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentoId { get; set; }

        public int? ProcesoIAId { get; set; }

        [Required, MaxLength(100)]
        public string TipoInsight { get; set; } = "general";

        [Column(TypeName = "nvarchar(max)")]
        public string Contenido { get; set; } = string.Empty;

        public double? Confianza { get; set; }

        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

        // Relaciones
        [ForeignKey("DocumentoId")]
        public virtual Documento Documento { get; set; } = null!;

        [ForeignKey("ProcesoIAId")]
        public virtual ProcesoIA? ProcesoIA { get; set; }
    }
}