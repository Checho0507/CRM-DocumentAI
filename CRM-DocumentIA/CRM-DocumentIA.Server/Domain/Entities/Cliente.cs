using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM_DocumentIA.Server.Domain.Entities;

public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    public string? Empresa { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;

    // Relación con Documentos (uno a muchos)
    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    // **NUEVA PROPIEDAD DE NAVEGACIÓN:** Relación 1:N con Insights
    public ICollection<Insight> Insights { get; set; } = new List<Insight>();
}
