namespace CRM_DocumentIA.Server.Application.DTOs.InsightsHisto
{
    public class InsightsHistoDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }

        public int chatId { get; set; }
    }
}
