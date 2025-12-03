namespace CRM_DocumentIA.Server.Application.DTOs.Chat
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}
