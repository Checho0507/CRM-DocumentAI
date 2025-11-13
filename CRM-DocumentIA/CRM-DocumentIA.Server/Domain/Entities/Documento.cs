using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities;

public class Documento
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; } // ✅ Cambiado de ClienteId a UsuarioId

    [Required, MaxLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoDocumento { get; set; }

    [MaxLength(500)]
    public string? RutaArchivo { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public bool Procesado { get; set; } = false;

    public string? ContenidoExtraido { get; set; }

    public byte[]? ArchivoDocumento { get; set; }

    public string? ArchivoMetadataJson { get; set; }

    public long? TamañoArchivo { get; set; } // ✅ Nuevo campo para tamaño

    // Relaciones
    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!; // ✅ Cambiado de Cliente a Usuario

    public virtual ICollection<ProcesoIA> ProcesosIA { get; set; } = new List<ProcesoIA>();
    public virtual ICollection<Insight> Insights { get; set; } = new List<Insight>();
}