// Domain/Entities/DocumentoCompartido.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities
{
    public class DocumentoCompartido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentoId { get; set; }

        [Required]
        public int UsuarioPropietarioId { get; set; }

        [Required]
        public int UsuarioCompartidoId { get; set; }

        public DateTime FechaCompartido { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Permiso { get; set; } = "lectura"; // "lectura", "escritura"

        [MaxLength(500)]
        public string? Mensaje { get; set; }

        // Relaciones
        [ForeignKey("DocumentoId")]
        public virtual Documento Documento { get; set; } = null!;

        [ForeignKey("UsuarioPropietarioId")]
        public virtual Usuario UsuarioPropietario { get; set; } = null!;

        [ForeignKey("UsuarioCompartidoId")]
        public virtual Usuario UsuarioCompartido { get; set; } = null!;
    }
}