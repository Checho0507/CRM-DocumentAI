namespace CRM_DocumentIA.Server.Application.DTOs.Insight
{
    public class InsightCreateDto
    {
        public int DocumentoId { get; set; }
        public int? ProcesoIAId { get; set; }
        public string TipoInsight { get; set; } = "general";
        public string Contenido { get; set; } = string.Empty;
        public double? Confianza { get; set; }
    }
}