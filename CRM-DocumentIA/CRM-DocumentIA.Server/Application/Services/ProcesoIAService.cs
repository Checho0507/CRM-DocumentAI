using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Application.DTOs.Documento;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class ProcesoIAService
    {
        private readonly IProcesoIARepository _procesoIARepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProcesoIAService> _logger;

        public ProcesoIAService(
            IProcesoIARepository procesoIARepository,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ProcesoIAService> logger)
        {
            _procesoIARepository = procesoIARepository;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<IEnumerable<ProcesoIA>> ObtenerTodosAsync()
            => await _procesoIARepository.ObtenerTodosAsync();

        public async Task<ProcesoIA?> ObtenerPorIdAsync(int id)
            => await _procesoIARepository.ObtenerPorIdAsync(id);

        public async Task<IEnumerable<ProcesoIA>> ObtenerPorDocumentoIdAsync(int documentoId)
            => await _procesoIARepository.ObtenerPorDocumentoIdAsync(documentoId);

        public async Task<IEnumerable<ProcesoIA>> ObtenerPorEstadoAsync(string estado)
            => await _procesoIARepository.ObtenerPorEstadoAsync(estado);

        public async Task AgregarAsync(ProcesoIA procesoIA)
            => await _procesoIARepository.AgregarAsync(procesoIA);

        public async Task ActualizarAsync(ProcesoIA procesoIA)
            => await _procesoIARepository.ActualizarAsync(procesoIA);

        public async Task EliminarAsync(int id)
            => await _procesoIARepository.EliminarAsync(id);

        public async Task<ProcesoIA> CrearProcesoAnalisisAsync(int documentoId)
        {
            var proceso = new ProcesoIA
            {
                DocumentoId = documentoId,
                TipoProcesamiento = "analisis_documento",
                Estado = "pendiente",
                FechaInicio = DateTime.UtcNow
            };

            await _procesoIARepository.AgregarAsync(proceso);
            return proceso;
        }

        public async Task<bool> MarcarComoProcesandoAsync(int procesoId)
        {
            var proceso = await _procesoIARepository.ObtenerPorIdAsync(procesoId);
            if (proceso == null) return false;

            proceso.Estado = "procesando";
            proceso.FechaInicio = DateTime.UtcNow;
            await _procesoIARepository.ActualizarAsync(proceso);
            return true;
        }

        public async Task<bool> MarcarComoCompletadoAsync(int procesoId, string resultadoJson, double? tiempoProcesamiento = null)
        {
            var proceso = await _procesoIARepository.ObtenerPorIdAsync(procesoId);
            if (proceso == null) return false;

            proceso.Estado = "completado";
            proceso.FechaFin = DateTime.UtcNow;
            proceso.ResultadoJson = resultadoJson;
            
            if (tiempoProcesamiento.HasValue)
            {
                proceso.TiempoProcesamientoSegundos = tiempoProcesamiento.Value;
            }
            else
            {
                proceso.TiempoProcesamientoSegundos = (proceso.FechaFin - proceso.FechaInicio)?.TotalSeconds;
            }

            await _procesoIARepository.ActualizarAsync(proceso);
            return true;
        }

        public async Task<bool> MarcarComoErrorAsync(int procesoId, string error)
        {
            var proceso = await _procesoIARepository.ObtenerPorIdAsync(procesoId);
            if (proceso == null) return false;

            proceso.Estado = "error";
            proceso.FechaFin = DateTime.UtcNow;
            proceso.Error = error;
            proceso.TiempoProcesamientoSegundos = (proceso.FechaFin - proceso.FechaInicio)?.TotalSeconds;

            await _procesoIARepository.ActualizarAsync(proceso);
            return true;
        }

        public async Task<ResultadoInsightRAG> GenerarInsightAsync(int documentoId, string pregunta)
        {
            try
            {
                var requestData = new
                {
                    documento_id = documentoId,
                    pregunta = pregunta,
                    tipo_consulta = "insight"
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("generar-insight", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var resultado = JsonSerializer.Deserialize<ResultadoInsightRAG>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return resultado ?? new ResultadoInsightRAG
                    {
                        Exito = false,
                        Error = "Respuesta inválida del servicio RAG"
                    };
                }
                else
                {
                    return new ResultadoInsightRAG
                    {
                        Exito = false,
                        Error = $"Error del servicio RAG: {response.StatusCode}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResultadoInsightRAG
                {
                    Exito = false,
                    Error = $"Error de conexión al servicio RAG: {ex.Message}"
                };
            }
        }

        // ✅ MÉTODO CORREGIDO - PROCESAR DOCUMENTO CON SERVICIO IA EXTERNO
        public async Task<ProcesamientoIADto> ProcesarDocumentoAsync(byte[] archivoBytes, string nombreArchivo)
        {
            try
            {
                _logger.LogInformation($"Iniciando procesamiento IA para archivo: {nombreArchivo}");

                var servicioIAUrl = _configuration["ServicioIA:BaseUrl"] ?? "http://localhost:8000";
                var endpoint = $"{servicioIAUrl}/ingest";

                _logger.LogInformation($"Enviando a servicio IA: {endpoint}");

                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(archivoBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "file", nombreArchivo);
                content.Add(new StringContent("openai"), "provider");
                content.Add(new StringContent("upload"), "source_name");

                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"✅ Respuesta exitosa del servicio IA: {responseContent}");

                    // Deserializar la respuesta completa
                    var respuestaCompleta = JsonSerializer.Deserialize<RespuestaRAGCompleta>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (respuestaCompleta != null && respuestaCompleta.Status == "ok" && respuestaCompleta.Result != null)
                    {
                        var resultado = respuestaCompleta.Result;
                        
                        // ✅ CORRECCIÓN CRÍTICA: Obtener el resumen correctamente
                        string resumen = !string.IsNullOrEmpty(resultado.ResumenDocumento) 
                            ? resultado.ResumenDocumento 
                            : (!string.IsNullOrEmpty(resultado.ContenidoExtraido) 
                                ? resultado.ContenidoExtraido 
                                : "No se pudo extraer contenido del documento");

                        // ✅ CORRECCIÓN: Serializar correctamente la metadata
                        string metadataJson = resultado.ArchivoMetadataJson != null 
                            ? JsonSerializer.Serialize(resultado.ArchivoMetadataJson, new JsonSerializerOptions 
                              { 
                                  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                  WriteIndented = false 
                              }): string.Empty;

                        var procesamientoDto = new ProcesamientoIADto 
                        {
                            Exito = true,
                            NumeroImagenes = resultado.NumeroImagenes,
                            Resumen = resumen, // ✅ Ahora usa el resumen correcto
                            MetadataAdicionalJson = metadataJson,
                            TiempoProcesamientoSegundos = respuestaCompleta.ElapsedSeconds,
                            DocumentId = resultado.DocumentId,
                            DocType = resultado.DocType,
                            TamañoArchivo = resultado.TamañoArchivo
                        };

                        _logger.LogInformation($"✅ Procesamiento IA completado para: {nombreArchivo}");
                        _logger.LogInformation($"   - Imágenes: {procesamientoDto.NumeroImagenes}");
                        _logger.LogInformation($"   - Resumen longitud: {procesamientoDto.Resumen?.Length ?? 0} caracteres");
                        _logger.LogInformation($"   - Tipo documento: {procesamientoDto.DocType}");
                        _logger.LogInformation($"   - Tiempo procesamiento: {procesamientoDto.TiempoProcesamientoSegundos}s");

                        return procesamientoDto;
                    }
                    else
                    {
                        _logger.LogWarning($"❌ Respuesta del servicio IA incompleta o con error para: {nombreArchivo}");
                        return new ProcesamientoIADto 
                        { 
                            Exito = false, 
                            Error = "Respuesta del servicio IA incompleta o con error" 
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Error del servicio IA. Status: {response.StatusCode}. Response: {errorContent}");
                    
                    return new ProcesamientoIADto 
                    { 
                        Exito = false, 
                        Error = $"Error del servicio IA: {response.StatusCode} - {errorContent}" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al comunicarse con el servicio IA para: {nombreArchivo}");
                return new ProcesamientoIADto 
                { 
                    Exito = false, 
                    Error = $"Error de conexión con el servicio IA: {ex.Message}" 
                };
            }
        }

        // Clases para deserializar la respuesta completa del RAG
        public class RespuestaRAGCompleta
        {
            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
            
            [JsonPropertyName("elapsed_seconds")]
            public double ElapsedSeconds { get; set; }
            
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = string.Empty;
            
            [JsonPropertyName("result")]
            public ResultadoRAG? Result { get; set; }
        }

        public class ResultadoRAG
        {
            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
            
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = string.Empty;
            
            [JsonPropertyName("document_id")]
            public string DocumentId { get; set; } = string.Empty;
            
            [JsonPropertyName("doc_type")]
            public string DocType { get; set; } = string.Empty;
            
            [JsonPropertyName("contenido_extraido")]
            public string ContenidoExtraido { get; set; } = string.Empty;
            
            [JsonPropertyName("resumen_documento")]
            public string ResumenDocumento { get; set; } = string.Empty;
            
            [JsonPropertyName("tamaño_archivo")]
            public long TamañoArchivo { get; set; }
            
            [JsonPropertyName("numero_imagenes")]
            public int NumeroImagenes { get; set; }
            
            [JsonPropertyName("imagenes_metadata")]
            public List<object> ImagenesMetadata { get; set; } = new();
            
            [JsonPropertyName("archivo_metadata_json")]
            public ArchivoMetadata? ArchivoMetadataJson { get; set; }
            
            [JsonPropertyName("elapsed_seconds")]
            public double ElapsedSeconds { get; set; }
        }

        public class ArchivoMetadata
        {
            [JsonPropertyName("document_id")]
            public string DocumentId { get; set; } = string.Empty;
            
            [JsonPropertyName("filename")]
            public string Filename { get; set; } = string.Empty;
            
            [JsonPropertyName("doc_type")]
            public string DocType { get; set; } = string.Empty;
            
            [JsonPropertyName("chunks")]
            public int Chunks { get; set; }
            
            [JsonPropertyName("vector_dim")]
            public int VectorDim { get; set; }
            
            [JsonPropertyName("source")]
            public string Source { get; set; } = string.Empty;
            
            [JsonPropertyName("numero_imagenes")]
            public int NumeroImagenes { get; set; }
        }

        public class ResultadoInsightRAG
        {
            public bool Exito { get; set; }
            public string Respuesta { get; set; } = string.Empty;
            public double Confianza { get; set; }
            public int? ProcesoIAId { get; set; }
            public string Error { get; set; } = string.Empty;
        }
    }

    public class ProcesamientoIADto
    {
        public bool Exito { get; set; }
        public int NumeroImagenes { get; set; }
        public string Resumen { get; set; } = string.Empty;
        public string? MetadataAdicionalJson { get; set; }
        public string? Error { get; set; }
        public double TiempoProcesamientoSegundos { get; set; }
        public string? DocumentId { get; set; }
        public string? DocType { get; set; }
        public long TamañoArchivo { get; set; }
    }
}