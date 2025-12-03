namespace CRM_DocumentIA.Server.Application.Dtos.Rag
{
    public class AskRagDto
    {
        public string Query { get; set; } = string.Empty;

        public int userId{ get; set; }

        public int ? chatId { get; set; }

    }
}
