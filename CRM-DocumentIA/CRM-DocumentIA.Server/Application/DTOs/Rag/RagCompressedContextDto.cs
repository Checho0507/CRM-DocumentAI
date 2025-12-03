namespace CRM_DocumentIA.Server.Application.Dtos.Rag
{
    public class RagCompressedContextDto
    {
        public string Text { get; set; } = string.Empty;

        public List<RagSourceInfoDto> SourceInfo { get; set; } = new();
    }

    public class RagSourceInfoDto
    {
        public string Id { get; set; } = string.Empty;

        public int ChunkIndex { get; set; }

        public string DocType { get; set; } = string.Empty;

        public string DocumentId { get; set; } = string.Empty;
    }
}
