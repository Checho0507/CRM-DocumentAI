// Application/DTOs/Documento/UsuarioBusquedaResponseDto.cs
namespace CRM_DocumentIA.Server.Application.DTOs.Documento
{
    public class UsuarioBusquedaResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Rol { get; set; }
    }
}