namespace CRM_DocumentIA.Server.Application.Dtos.Rag
{
    public class RagResponseDto
    {
        public string Query { get; set; } = string.Empty;

        public string DocType { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public List<RagSourceDto> Sources { get; set; } = new();

        public List<RagCompressedContextDto> CompressedContext { get; set; } = new();

        public double ElapsedSeconds { get; set; }
    }
}
