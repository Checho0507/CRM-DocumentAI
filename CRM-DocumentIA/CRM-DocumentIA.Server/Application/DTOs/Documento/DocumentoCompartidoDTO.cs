namespace CRM_DocumentIA.Server.Application.DTOs.Documento
{
    public class DocumentoCompartidoDto
    {
        public int Id { get; set; }
        public int DocumentoId { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public int UsuarioPropietarioId { get; set; }
        public string NombrePropietario { get; set; } = string.Empty;
        public int UsuarioCompartidoId { get; set; }
        public string NombreUsuarioCompartido { get; set; } = string.Empty;
        public DateTime FechaCompartido { get; set; }
        public string Permiso { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
    }
}