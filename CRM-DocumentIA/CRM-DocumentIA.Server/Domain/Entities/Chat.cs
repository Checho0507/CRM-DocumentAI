using System.ComponentModel.DataAnnotations;

namespace CRM_DocumentIA.Server.Domain.Entities
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  // Usuario dueño del chat

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Title { get; set; } // opcional: nombre del chat

        // Relación 1:N con InsightsHisto
        public ICollection<InsightsHisto> Insights { get; set; } = new List<InsightsHisto>();
    }
}
