namespace CRM_DocumentIA.Server.Application.DTOs.InsightsHisto
{
    public class CreateInsightsHistoDto
    {
        public int UserId { get; set; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }
}
