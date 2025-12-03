using CRM_DocumentIA.Server.Application.DTOs.InsightsHisto;
using System.Collections.Generic;

namespace CRM_DocumentIA.Server.Application.DTOs.Chat
{
    public class ChatDetailDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }

        public List<InsightsHistoDto> Insights { get; set; } = new();
    }
}
