using System.ComponentModel.DataAnnotations;

namespace CRM_DocumentIA.Server.Application.DTOs.Documento
{
    public class CompartirDocumentoDTO
    {
        [Required(ErrorMessage = "El ID del documento es requerido")]
        public int DocumentoId { get; set; }

        [Required(ErrorMessage = "El ID del usuario a compartir es requerido")]
        public int UsuarioCompartidoId { get; set; }

        [Required(ErrorMessage = "El permiso es requerido")]
        [RegularExpression("^(lectura|escritura)$", ErrorMessage = "El permiso debe ser 'lectura' o 'escritura'")]
        public string Permiso { get; set; } = "lectura";

        [MaxLength(500, ErrorMessage = "El mensaje no puede exceder los 500 caracteres")]
        public string? Mensaje { get; set; }
    }
}