// Domain/Entities/Documento.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities;

public class Documento
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required, MaxLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoDocumento { get; set; }

    [MaxLength(500)]
    public string? RutaArchivo { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

    public bool Procesado { get; set; } = false;


    public byte[]? ArchivoDocumento { get; set; }

    public string? ArchivoMetadataJson { get; set; }

    public long? TamañoArchivo { get; set; }

    // ✅ NUEVOS CAMPOS PARA EL PROCESAMIENTO IA
    public int? NumeroImagenes { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? ResumenDocumento { get; set; }
    
    [Required, MaxLength(50)]
    public string EstadoProcesamiento { get; set; } = "pendiente"; // "pendiente", "procesando", "completado", "error"
    
    public DateTime? FechaProcesamiento { get; set; }
    
    [MaxLength(1000)]
    public string? UrlServicioIA { get; set; }

    public string? ErrorProcesamiento { get; set; }

    // Relaciones
    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;

    public virtual ICollection<ProcesoIA> ProcesosIA { get; set; } = new List<ProcesoIA>();
    public virtual ICollection<Insight> Insights { get; set; } = new List<Insight>();
    
    // ✅ NUEVO: Relación con Documentos Compartidos
    public virtual ICollection<DocumentoCompartido> Compartidos { get; set; } = new List<DocumentoCompartido>();
}