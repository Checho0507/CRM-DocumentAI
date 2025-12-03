using CRM_DocumentIA.Server.Application.Dtos.Rag;

public interface IRagClient
{
    Task<RagResponseDto?> AskAsync(RagRequestDto request);
}