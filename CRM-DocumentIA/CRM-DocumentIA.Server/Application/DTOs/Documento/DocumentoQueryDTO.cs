// Application/DTOs/Documento/DocumentoQueryDto.cs
namespace CRM_DocumentIA.Server.Application.DTOs.Documento
{
    public class DocumentoQueryDto
    {
        public string? Estado { get; set; }
        public int? UsuarioId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? TipoArchivo { get; set; }
        public string? OrdenarPor { get; set; } = "FechaSubida"; // Cambiado a FechaSubida para coincidir con tu entity
        public bool OrdenDescendente { get; set; } = true;
        public int Pagina { get; set; } = 1;
        public int TamanioPagina { get; set; } = 20;
    }
}