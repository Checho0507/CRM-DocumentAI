// Domain/Entities/Usuario.cs
using CRM_DocumentIA.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace CRM_DocumentIA.Server.Domain.Entities;

public class Usuario
{
    public Usuario(Email email, string nombre, string passwordHash, int rolId)
    {
        Email = email;
        Nombre = nombre;
        PasswordHash = passwordHash;
        RolId = rolId;
    }

    // Constructor vacío para EF Core
    public Usuario() { }

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public Email Email { get; set; } = null!;

    public string PasswordHash { get; set; } = string.Empty;

    public int RolId { get; set; }
    public Rol Rol { get; set; } = null!; 
    
    public bool DobleFactorActivado { get; internal set; }

    // ✅ Relación con Documentos propios
    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    // ✅ NUEVO: Documentos compartidos CONMIGO
    public virtual ICollection<DocumentoCompartido> DocumentosCompartidosConmigo { get; set; } = new List<DocumentoCompartido>();

    // ✅ NUEVO: Documentos que YO he compartido
    public virtual ICollection<DocumentoCompartido> DocumentosQueHeCompartido { get; set; } = new List<DocumentoCompartido>();
}