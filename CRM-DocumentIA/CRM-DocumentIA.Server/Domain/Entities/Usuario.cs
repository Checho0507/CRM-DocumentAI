// Domain/Entities/Usuario.cs (Actualización)

using CRM_DocumentIA.Domain.ValueObjects; // Añade el using

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // **USO DEL VALUE OBJECT**
    // Nota: Entity Framework Core sabrá mapear el objeto Email al campo string en la DB.
    // Esto requiere una configuración adicional en el DbContext (ver más adelante).
    public Email Email { get; set; } = null!; // Ya no es string

    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "usuario";
    public bool DobleFactorActivado { get; internal set; }

    // ... (otras propiedades)
}