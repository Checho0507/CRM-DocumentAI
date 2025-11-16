namespace CRM_DocumentIA.Server.Application.DTOs.Auth

{
    // DTO para la información del usuario devuelta al frontend
    public class UsuarioInfoDTO
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
    }

    // DTO de respuesta final de login
    public class RespuestaAuthDTO
    {
        public string Token { get; set; } = string.Empty; // El JWT
        public UsuarioInfoDTO Usuario { get; set; } = null!; // Los datos del usuario
        public bool DobleFactorActivado { get; internal set; }
    }
}