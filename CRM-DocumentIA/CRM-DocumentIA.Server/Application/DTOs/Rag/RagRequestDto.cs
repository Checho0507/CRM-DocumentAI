namespace CRM_DocumentIA.Server.Application.Dtos.Rag
{
    public class RagRequestDto
    {
        public string Query { get; set; } = string.Empty;

        // Tipo de documento: "factura", "documento", etc.
        public string ? DocType { get; set; } = string.Empty;

        public string provider { get; set; } = string.Empty;
    }
}
