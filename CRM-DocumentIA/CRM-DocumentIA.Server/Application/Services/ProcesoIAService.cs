using System.Text;
using System.Text.Json;
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
            
            // Configurar timeout más largo para procesamiento de documentos
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

        // Método específico para crear proceso de análisis
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

        // Método para marcar proceso como procesando
        public async Task<bool> MarcarComoProcesandoAsync(int procesoId)
        {
            var proceso = await _procesoIARepository.ObtenerPorIdAsync(procesoId);
            if (proceso == null) return false;

            proceso.Estado = "procesando";
            proceso.FechaInicio = DateTime.UtcNow;
            await _procesoIARepository.ActualizarAsync(proceso);
            return true;
        }

        // Método para marcar proceso como completado
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

        // Método para marcar proceso como error
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
                // Aquí integras con tu servicio RAG externo
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

        public class ResultadoInsightRAG
        {
            public bool Exito { get; set; }
            public string Respuesta { get; set; } = string.Empty;
            public double Confianza { get; set; }
            public int? ProcesoIAId { get; set; }
            public string Error { get; set; } = string.Empty;
        }

        // ✅ MÉTODO PARA PROCESAR DOCUMENTO CON SERVICIO IA EXTERNO
        public async Task<ProcesamientoIADto> ProcesarDocumentoAsync(byte[] archivoBytes, string nombreArchivo)
        {
            try
            {
                _logger.LogInformation($"Iniciando procesamiento IA para archivo: {nombreArchivo}");

                // Obtener la URL del servicio IA desde configuración
                var servicioIAUrl = _configuration["ServicioIA:BaseUrl"] ?? "http://localhost:8000";
                var endpoint = $"{servicioIAUrl}/api/procesar-documento";

                _logger.LogInformation($"Enviando a servicio IA: {endpoint}");

                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(archivoBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, "file", nombreArchivo);

                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Respuesta exitosa del servicio IA: {responseContent}");

                    var resultado = JsonSerializer.Deserialize<ProcesamientoIADto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (resultado != null)
                    {
                        resultado.Exito = true;
                        _logger.LogInformation($"Procesamiento IA completado para: {nombreArchivo}. Imágenes: {resultado.NumeroImagenes}");
                        return resultado;
                    }
                    else
                    {
                        _logger.LogWarning($"No se pudo deserializar respuesta del servicio IA para: {nombreArchivo}");
                        return new ProcesamientoIADto 
                        { 
                            Exito = false, 
                            Error = "No se pudo interpretar la respuesta del servicio IA" 
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error del servicio IA. Status: {response.StatusCode}. Response: {errorContent}");
                    
                    return new ProcesamientoIADto 
                    { 
                        Exito = false, 
                        Error = $"Error del servicio IA: {response.StatusCode} - {errorContent}" 
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al comunicarse con el servicio IA para: {nombreArchivo}");
                return new ProcesamientoIADto 
                { 
                    Exito = false, 
                    Error = $"Error de conexión con el servicio IA: {ex.Message}" 
                };
            }
        }
    }
}