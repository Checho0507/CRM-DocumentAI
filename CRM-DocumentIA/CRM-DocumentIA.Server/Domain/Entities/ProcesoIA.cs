using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities;


public class ProcesoIA
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentoId { get; set; }

    [MaxLength(50)]
    public string? TipoProceso { get; set; } // OCR, Clasificación, Extracción

    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Procesando, Completado, Error

    public string? Resultado { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    // Relación con Documento (muchos a uno)
    [ForeignKey("DocumentoId")]
    public virtual Documento Documento { get; set; } = null!;
}