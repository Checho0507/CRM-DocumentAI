// Application/DTOs/Usuario/UsuarioDTO.cs

namespace CRM_DocumentIA.Server.Application.DTOs.Usuario
{
    // DTO para enviar datos del perfil al frontend (sin PasswordHash)
    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public bool DobleFactorActivado { get; set; }
    }

    // DTO para recibir datos de actualización
    public class ActualizacionUsuarioDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public bool? DobleFactorActivado { get; set; }
    }
}