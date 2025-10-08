using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CRM_DocumentIA.server.Models;

namespace CRM_DocumentIA.Server.Models;

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
    public string? TipoDocumento { get; set; } // Factura, Contrato, etc.

    [MaxLength(500)]
    public string? RutaArchivo { get; set; }

    public DateTime FechaSubida { get; set; } = DateTime.Now;
    public bool Procesado { get; set; } = false;

    public string? ContenidoExtraido { get; set; }

    // Relación con Cliente (muchos a uno)
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;

    // Relación con ProcesosAI (uno a muchos)
    public virtual ICollection<ProcesoIA> ProcesosIA { get; set; } = new List<ProcesoIA>();
}
