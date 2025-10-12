/// <summary>
/// Datos requeridos para registrar un nuevo usuario.
/// </summary>
public class RegistroDTO
{
    /// <summary>Nombre del usuario</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Correo electrónico</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña de acceso</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Si es true, se activa la verificación en dos pasos (2FA)</summary>
    public bool DobleFactorActivado { get; set; }
}