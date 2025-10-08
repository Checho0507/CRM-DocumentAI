// Application/DTOs/Auth/RegistroDTO.cs

using System.ComponentModel.DataAnnotations;

namespace CRM_DocumentIA.Server.Application.DTOs.Auth
{
    public class RegistroDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}