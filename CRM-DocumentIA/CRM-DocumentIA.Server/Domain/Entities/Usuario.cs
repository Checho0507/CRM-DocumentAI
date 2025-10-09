// Domain/Entities/Usuario.cs (Actualización)

using CRM_DocumentIA.Domain.ValueObjects; // Añade el using

public class Usuario
{
    public Usuario(Email email, string nombre, string passwordHash, string rol)
    {
        Email = email;
        Nombre = nombre;
        PasswordHash = passwordHash;
        Rol = rol;
    }

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public Email Email { get; set; } = null!; // Ya no es string

    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "usuario";
    public bool DobleFactorActivado { get; internal set; }

    // ... (otras propiedades)
}