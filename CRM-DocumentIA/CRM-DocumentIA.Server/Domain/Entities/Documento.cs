using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities;

public class Documento
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Required]
    [MaxLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoDocumento { get; set; }

    [MaxLength(500)]
    public string? RutaArchivo { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.Now;
    public bool Procesado { get; set; } = false;

    public string? ContenidoExtraido { get; set; }

    // 🔹 Nuevo campo: contenido binario del archivo
    public byte[]? ArchivoDocumento { get; set; }

    // 🔹 Nuevo campo: JSON con metadatos del documento
    public string? ArchivoMetadataJson { get; set; }

    // Relaciones
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<ProcesoIA> ProcesosIA { get; set; } = new List<ProcesoIA>();

    public ICollection<Insight> Insights { get; set; } = new List<Insight>();
}
