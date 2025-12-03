using System.Net.Http.Json;
using CRM_DocumentIA.Server.Application.Dtos.Rag;

namespace CRM_DocumentIA.Server.Infrastructure.Rag
{
    public class RagClient : IRagClient
    {
        private readonly HttpClient _httpClient;

        public RagClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RagResponseDto?> AskAsync(RagRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("query", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error en RAG: {errorText}");
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<RagResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al conectar con el RAG:");
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
