using Microsoft.AspNetCore.Http;

namespace CRM_DocumentIA.Server.Application.DTOs.Documento
{
    public class DocumentoUploadDto
    {
        public required IFormFile Archivo { get; set; }
        public int UsuarioId { get; set; }
    }
}