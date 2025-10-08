// Application/DTOs/Insight/InsightDTO.cs

namespace CRM_DocumentIA.Server.Application.DTOs.Insight
{
    public class InsightDTO
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime GeneradoEn { get; set; }
    }
}