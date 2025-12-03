namespace CRM_DocumentIA.Server.Application.Dtos.Rag
{
    public class RagSourceDto
    {
        public int ChunkIndex { get; set; }

        public string DocType { get; set; } = string.Empty;

        public string DocumentId { get; set; } = string.Empty;

        public string Filename { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string TextExcerpt { get; set; } = string.Empty;
    }
}
