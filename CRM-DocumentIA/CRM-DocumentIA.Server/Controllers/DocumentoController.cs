using CRM_DocumentIA.Server.Application.DTOs.Analytics;
using CRM_DocumentIA.Server.Application.DTOs.Documento;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly DocumentoService _documentoService;
        private readonly ProcesoIAService _procesoIAService;
        private readonly AnalyticsService _analyticsService;

        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentoController> _logger;

        public DocumentoController(
            DocumentoService documentoService,
            ProcesoIAService procesoIAService,
            AnalyticsService analyticsService,

        IWebHostEnvironment environment,
            ILogger<DocumentoController> logger)
        {
            _documentoService = documentoService;
            _procesoIAService = procesoIAService;
            _analyticsService = analyticsService;
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] DocumentoUploadDto dto)
        {
            try
            {
                if (dto?.Archivo == null || dto.Archivo.Length == 0)
                    return BadRequest(new { mensaje = "Archivo no válido" });

                if (dto.UsuarioId <= 0)
                    return BadRequest(new { mensaje = "UsuarioId debe ser mayor a 0" });

                _logger.LogInformation($"📤 Iniciando upload y procesamiento de documento: {dto.Archivo.FileName} para usuario: {dto.UsuarioId}");

                // 1. Convertir archivo a bytes
                byte[] archivoBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await dto.Archivo.CopyToAsync(memoryStream);
                    archivoBytes = memoryStream.ToArray();
                }

                // 2. Procesar documento con servicio IA (esto se hace primero)
                _logger.LogInformation($"🤖 Enviando documento al servicio IA: {dto.Archivo.FileName}");
                var resultadoIA = await _procesoIAService.ProcesarDocumentoAsync(archivoBytes, dto.Archivo.FileName);

                if (!resultadoIA.Exito)
                {
                    _logger.LogError($"❌ Error en procesamiento IA: {resultadoIA.Error}");
                    return StatusCode(500, new { 
                        mensaje = "Error en el procesamiento del documento", 
                        error = resultadoIA.Error 
                    });
                }

                _logger.LogInformation($"✅ Procesamiento IA exitoso");
                _logger.LogInformation($"   - Imágenes: {resultadoIA.NumeroImagenes}");
                _logger.LogInformation($"   - Tamaño: {resultadoIA.TamañoArchivo} bytes");
                _logger.LogInformation($"   - Tipo: {resultadoIA.DocType}");
                _logger.LogInformation($"   - Tiempo: {resultadoIA.TiempoProcesamientoSegundos}s");
                _logger.LogInformation($"   - Resumen longitud: {resultadoIA.Resumen?.Length ?? 0} caracteres");
                _logger.LogInformation($"   - Resumen preview: {resultadoIA.Resumen?.Substring(0, Math.Min(100, resultadoIA.Resumen.Length))}...");

                // 3. Guardar archivo físicamente
                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{dto.Archivo.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, archivoBytes);

                // 4. Crear Documento ya procesado con TODOS los campos
                var documento = new Documento
                {
                    UsuarioId = dto.UsuarioId,
                    NombreArchivo = dto.Archivo.FileName,
                    TipoDocumento = Path.GetExtension(dto.Archivo.FileName),
                    RutaArchivo = filePath,
                    FechaSubida = DateTime.UtcNow,
                    Procesado = true,
                    ArchivoDocumento = archivoBytes,
                    TamañoArchivo = resultadoIA.TamañoArchivo > 0 ? resultadoIA.TamañoArchivo : dto.Archivo.Length,
                    EstadoProcesamiento = "completado",
                    NumeroImagenes = resultadoIA.NumeroImagenes,
                    ResumenDocumento = resultadoIA.Resumen, // ✅ Ahora se guarda el resumen correctamente
                    ArchivoMetadataJson = resultadoIA.MetadataAdicionalJson,
                    FechaProcesamiento = DateTime.UtcNow,
                    UrlServicioIA = "http://localhost:8000/ingest"
                };

                await _documentoService.AgregarAsync(documento);
                _logger.LogInformation($"💾 Documento guardado con ID: {documento.Id}");

                // 5. Crear ProcesoIA como COMPLETADO con TODOS los campos
                var procesoIA = new ProcesoIA
                {
                    DocumentoId = documento.Id,
                    TipoProcesamiento = "analisis_documento",
                    Estado = "completado",
                    FechaInicio = DateTime.UtcNow.AddSeconds(-resultadoIA.TiempoProcesamientoSegundos),
                    FechaFin = DateTime.UtcNow,
                    ResultadoJson = JsonSerializer.Serialize(new
                    {
                        numeroImagenes = resultadoIA.NumeroImagenes,
                        resumen = resultadoIA.Resumen,
                        metadata = resultadoIA.MetadataAdicionalJson,
                        documentId = resultadoIA.DocumentId,
                        docType = resultadoIA.DocType,
                        tamañoArchivo = resultadoIA.TamañoArchivo,
                        tiempoProcesamiento = resultadoIA.TiempoProcesamientoSegundos,
                        fechaProcesamiento = DateTime.UtcNow
                    }, new JsonSerializerOptions { WriteIndented = true }),
                    TiempoProcesamientoSegundos = resultadoIA.TiempoProcesamientoSegundos,
                    UrlServicio = "http://localhost:8000/ingest"
                };

                await _procesoIAService.AgregarAsync(procesoIA);
                _logger.LogInformation($"🔗 ProcesoIA creado con ID: {procesoIA.Id}");

                _logger.LogInformation($"🎉 Documento procesado y guardado completamente. ID: {documento.Id}");

                return Ok(new
                {
                    mensaje = "Documento procesado y guardado correctamente",
                    documentoId = documento.Id,
                    procesoIAId = procesoIA.Id,
                    usuarioId = dto.UsuarioId,
                    estado = "completado",
                    numeroImagenes = resultadoIA.NumeroImagenes,
                    resumen = resultadoIA.Resumen,
                    docType = resultadoIA.DocType,
                    documentId = resultadoIA.DocumentId,
                    tiempoProcesamiento = resultadoIA.TiempoProcesamientoSegundos,
                    fechaProcesamiento = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error en upload de documento: {ex.Message}");
                return StatusCode(500, new { 
                    mensaje = "Error interno al procesar el archivo", 
                    error = ex.Message 
                });
            }
        }

        // ENDPOINT PARA PROCESAR DOCUMENTOS EXISTENTES PENDIENTES
        [HttpPost("{id}/procesar")]
        public async Task<IActionResult> ProcesarDocumentoExistente(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                if (documento.EstadoProcesamiento == "completado")
                    return BadRequest(new { mensaje = "El documento ya está procesado" });

                _logger.LogInformation($"🔄 Procesando documento existente ID: {id} - {documento.NombreArchivo}");

                // Obtener bytes del archivo
                byte[] archivoBytes;
                if (documento.ArchivoDocumento != null && documento.ArchivoDocumento.Length > 0)
                {
                    archivoBytes = documento.ArchivoDocumento;
                }
                else if (!string.IsNullOrEmpty(documento.RutaArchivo) && System.IO.File.Exists(documento.RutaArchivo))
                {
                    archivoBytes = await System.IO.File.ReadAllBytesAsync(documento.RutaArchivo);
                }
                else
                {
                    return NotFound(new { mensaje = "No se pudo encontrar el archivo para procesar" });
                }

                // Procesar con IA
                _logger.LogInformation($"🤖 Enviando documento existente al servicio IA: {documento.NombreArchivo}");
                var resultadoIA = await _procesoIAService.ProcesarDocumentoAsync(archivoBytes, documento.NombreArchivo);

                if (resultadoIA.Exito)
                {
                    _logger.LogInformation($"✅ Procesamiento IA exitoso para documento existente ID: {id}");
                    _logger.LogInformation($"   - Resumen longitud: {resultadoIA.Resumen?.Length ?? 0} caracteres");

                    // Actualizar documento con resultados
                    documento.EstadoProcesamiento = "completado";
                    documento.Procesado = true;
                    documento.NumeroImagenes = resultadoIA.NumeroImagenes;
                    documento.ResumenDocumento = resultadoIA.Resumen; // ✅ Resumen guardado correctamente
                    documento.ArchivoMetadataJson = resultadoIA.MetadataAdicionalJson;
                    documento.FechaProcesamiento = DateTime.UtcNow;
                    documento.UrlServicioIA = "http://localhost:8000/ingest";

                    await _documentoService.ActualizarAsync(documento);

                    // Crear proceso IA completado
                    var procesoIA = new ProcesoIA
                    {
                        DocumentoId = documento.Id,
                        TipoProcesamiento = "analisis_documento",
                        Estado = "completado",
                        FechaInicio = DateTime.UtcNow.AddSeconds(-resultadoIA.TiempoProcesamientoSegundos),
                        FechaFin = DateTime.UtcNow,
                        ResultadoJson = JsonSerializer.Serialize(new
                        {
                            numeroImagenes = resultadoIA.NumeroImagenes,
                            resumen = resultadoIA.Resumen,
                            metadata = resultadoIA.MetadataAdicionalJson,
                            documentId = resultadoIA.DocumentId,
                            docType = resultadoIA.DocType,
                            tamañoArchivo = resultadoIA.TamañoArchivo,
                            tiempoProcesamiento = resultadoIA.TiempoProcesamientoSegundos,
                            fechaProcesamiento = DateTime.UtcNow
                        }, new JsonSerializerOptions { WriteIndented = true }),
                        TiempoProcesamientoSegundos = resultadoIA.TiempoProcesamientoSegundos,
                        UrlServicio = "http://localhost:8000/ingest"
                    };

                    await _procesoIAService.AgregarAsync(procesoIA);

                    _logger.LogInformation($"🎉 Documento existente procesado correctamente. ID: {documento.Id}");

                    return Ok(new
                    {
                        mensaje = "Documento procesado correctamente",
                        documentoId = documento.Id,
                        procesoIAId = procesoIA.Id,
                        estado = "completado",
                        numeroImagenes = resultadoIA.NumeroImagenes,
                        resumen = resultadoIA.Resumen,
                        tiempoProcesamiento = resultadoIA.TiempoProcesamientoSegundos
                    });
                }
                else
                {
                    _logger.LogError($"❌ Error en procesamiento IA para documento existente ID: {id}: {resultadoIA.Error}");

                    // Marcar como error
                    documento.EstadoProcesamiento = "error";
                    documento.ErrorProcesamiento = resultadoIA.Error;
                    await _documentoService.ActualizarAsync(documento);

                    return StatusCode(500, new
                    {
                        mensaje = "Error en el procesamiento del documento",
                        error = resultadoIA.Error
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al procesar documento existente {id}");
                return StatusCode(500, new
                {
                    mensaje = "Error al procesar documento",
                    error = ex.Message
                });
            }
        }

        // ... (los demás métodos del controller se mantienen igual)
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(int usuarioId)
        {
            try
            {
                var documentos = await _documentoService.ObtenerPorUsuarioIdAsync(usuarioId);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener documentos del usuario {usuarioId}");
                return StatusCode(500, new { mensaje = "Error al obtener documentos del usuario", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                return Ok(documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener documento {id}");
                return StatusCode(500, new { mensaje = "Error al obtener documento", error = ex.Message });
            }
        }

        [HttpGet("estado/{estado}")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            try
            {
                var documentos = await _documentoService.ObtenerPorEstadoAsync(estado);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener documentos con estado {estado}");
                return StatusCode(500, new { mensaje = "Error al obtener documentos", error = ex.Message });
            }
        }

        [HttpGet("{id}/procesos")]
        public async Task<IActionResult> GetProcesosPorDocumento(int id)
        {
            try
            {
                var procesos = await _procesoIAService.ObtenerPorDocumentoIdAsync(id);
                return Ok(procesos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener procesos del documento {id}");
                return StatusCode(500, new { mensaje = "Error al obtener procesos del documento", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                // Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(documento.RutaArchivo) && System.IO.File.Exists(documento.RutaArchivo))
                {
                    System.IO.File.Delete(documento.RutaArchivo);
                }

                await _documentoService.EliminarAsync(id);
                return Ok(new { mensaje = "Documento eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar documento {id}");
                return StatusCode(500, new { mensaje = "Error al eliminar documento", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var documentos = await _documentoService.ObtenerTodosAsync();
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los documentos");
                return StatusCode(500, new { mensaje = "Error al obtener documentos", error = ex.Message });
            }
        }

        [HttpGet("estado")]
        public async Task<IActionResult> GetEstados()
        {
            try
            {
                var estados = new[]
                {
                    new { Valor = "pendiente", Texto = "Pendiente" },
                    new { Valor = "procesando", Texto = "Procesando" },
                    new { Valor = "completado", Texto = "Completado" },
                    new { Valor = "error", Texto = "Error" }
                };

                return Ok(estados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados de documentos");
                return StatusCode(500, new { mensaje = "Error al obtener estados", error = ex.Message });
            }
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] ActualizarEstadoDocumentoDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Estado))
                    return BadRequest(new { mensaje = "Estado no válido" });

                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                documento.EstadoProcesamiento = dto.Estado;
                documento.ErrorProcesamiento = dto.MensajeError;
                
                await _documentoService.ActualizarAsync(documento);
                return Ok(new { mensaje = "Estado actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar estado del documento {id}");
                return StatusCode(500, new { mensaje = "Error al actualizar estado", error = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                byte[] archivoBytes;
                string contentType = "application/octet-stream";
                string nombreArchivo = documento.NombreArchivo;

                if (documento.ArchivoDocumento != null && documento.ArchivoDocumento.Length > 0)
                {
                    archivoBytes = documento.ArchivoDocumento;
                }
                else if (!string.IsNullOrEmpty(documento.RutaArchivo) && System.IO.File.Exists(documento.RutaArchivo))
                {
                    archivoBytes = await System.IO.File.ReadAllBytesAsync(documento.RutaArchivo);
                }
                else
                {
                    return NotFound(new { mensaje = "Archivo no encontrado" });
                }

                // Determinar content type según extensión
                if (nombreArchivo.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    contentType = "application/pdf";
                else if (nombreArchivo.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                else if (nombreArchivo.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                else if (nombreArchivo.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || nombreArchivo.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/jpeg";
                else if (nombreArchivo.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    contentType = "image/png";

                return File(archivoBytes, contentType, nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al descargar documento {id}");
                return StatusCode(500, new { mensaje = "Error al descargar documento", error = ex.Message });
            }
        }

        [HttpGet("stats/usuario/{usuarioId}")]
        public async Task<IActionResult> GetStatsByUsuario(int usuarioId)
        {
            try
            {
                var stats = await _documentoService.ObtenerEstadisticasPorUsuarioAsync(usuarioId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener estadísticas del usuario {usuarioId}");
                return StatusCode(500, new { mensaje = "Error al obtener estadísticas", error = ex.Message });
            }
        }

        // ENDPOINT PARA OBTENER DOCUMENTOS PENDIENTES DE PROCESAR
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            try
            {
                var documentos = await _documentoService.ObtenerPorEstadoAsync("pendiente");
                return Ok(new
                {
                    total = documentos.Count(),
                    documentos = documentos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos pendientes");
                return StatusCode(500, new { mensaje = "Error al obtener documentos pendientes", error = ex.Message });
            }
        }

        // ENDPOINT PARA PROCESAR TODOS LOS DOCUMENTOS PENDIENTES
        [HttpPost("procesar-pendientes")]
        public async Task<IActionResult> ProcesarTodosPendientes()
        {
            try
            {
                var documentosPendientes = await _documentoService.ObtenerPorEstadoAsync("pendiente");
                var documentosList = documentosPendientes.ToList();

                if (!documentosList.Any())
                    return Ok(new { mensaje = "No hay documentos pendientes para procesar" });

                _logger.LogInformation($"Iniciando procesamiento de {documentosList.Count} documentos pendientes");

                var resultados = new List<object>();
                var procesadosExitosos = 0;
                var errores = 0;

                foreach (var documento in documentosList)
                {
                    try
                    {
                        // Usar el endpoint de procesar documento existente
                        var resultado = await ProcesarDocumentoExistente(documento.Id);
                        if (resultado is OkObjectResult)
                        {
                            procesadosExitosos++;
                        }
                        else
                        {
                            errores++;
                        }

                        resultados.Add(new
                        {
                            documentoId = documento.Id,
                            nombre = documento.NombreArchivo,
                            success = resultado is OkObjectResult
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error al procesar documento pendiente {documento.Id}");
                        errores++;
                        resultados.Add(new
                        {
                            documentoId = documento.Id,
                            nombre = documento.NombreArchivo,
                            success = false,
                            error = ex.Message
                        });
                    }

                    // Pequeña pausa para no saturar el servicio IA
                    await Task.Delay(1000);
                }

                return Ok(new
                {
                    mensaje = $"Procesamiento completado. Exitosos: {procesadosExitosos}, Errores: {errores}",
                    total = documentosList.Count,
                    exitosos = procesadosExitosos,
                    errores = errores,
                    detalles = resultados
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar documentos pendientes");
                return StatusCode(500, new { mensaje = "Error al procesar documentos pendientes", error = ex.Message });
            }
        }
    }

    // DTOs para los endpoints
    public class ActualizarEstadoDocumentoDto
    {
        public string Estado { get; set; } = string.Empty;
        public string? MensajeError { get; set; }
    }
}