using CRM_DocumentIA.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace CRM_DocumentIA.Server.Domain.Entities;

public class Usuario
{
    public Usuario(Email email, string nombre, string passwordHash, string rol)
    {
        Email = email;
        Nombre = nombre;
        PasswordHash = passwordHash;
        Rol = rol;
    }

    // Constructor vacío para EF Core
    public Usuario() { }

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public Email Email { get; set; } = null!;

    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "usuario";
    public bool DobleFactorActivado { get; internal set; }

    // ✅ Agregar la relación inversa con Documento
    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();

    // ... (otras propiedades que ya tengas)
}