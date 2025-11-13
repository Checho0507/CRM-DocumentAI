using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Application.DTOs.Documento;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly DocumentoService _documentoService;
        private readonly ProcesoIAService _procesoIAService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DocumentoController> _logger;

        public DocumentoController(
            DocumentoService documentoService,
            ProcesoIAService procesoIAService,
            IWebHostEnvironment environment,
            ILogger<DocumentoController> logger)
        {
            _documentoService = documentoService;
            _procesoIAService = procesoIAService;
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

                _logger.LogInformation($"Iniciando upload de documento: {dto.Archivo.FileName} para usuario: {dto.UsuarioId}");

                // 1. Subir documento básico
                var documento = await _documentoService.SubirDocumentoAsync(dto.Archivo, dto.UsuarioId, _environment);
                _logger.LogInformation($"Documento guardado con ID: {documento.Id}");

                // 2. Crear ProcesoIA automáticamente
                var procesoIA = await _procesoIAService.CrearProcesoAnalisisAsync(documento.Id);
                _logger.LogInformation($"ProcesoIA creado con ID: {procesoIA.Id}");

                // 3. Iniciar procesamiento en segundo plano (no bloqueante)
                _ = Task.Run(async () => await ProcesarDocumentoCompleto(documento, procesoIA));

                return Ok(new
                {
                    mensaje = "Archivo subido correctamente. Procesamiento en curso.",
                    documentoId = documento.Id,
                    procesoIAId = procesoIA.Id,
                    usuarioId = dto.UsuarioId,
                    estado = "procesando"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en upload de documento: {ex.Message}");
                return StatusCode(500, new { mensaje = "Error interno al subir el archivo", error = ex.Message });
            }
        }

        private async Task ProcesarDocumentoCompleto(Documento documento, ProcesoIA procesoIA)
        {
            try
            {
                _logger.LogInformation($"Iniciando procesamiento completo para documento ID: {documento.Id}");

                // 1. Actualizar estados a "procesando"
                await _documentoService.MarcarComoProcesandoAsync(documento.Id);
                await _procesoIAService.MarcarComoProcesandoAsync(procesoIA.Id);

                // 2. Leer archivo para enviar al servicio IA
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
                    throw new FileNotFoundException("No se pudo encontrar el archivo para procesar");
                }

                // 3. Llamar al servicio IA externo usando ProcesoIAService
                var resultadoIA = await _procesoIAService.ProcesarDocumentoAsync(archivoBytes, documento.NombreArchivo);

                if (resultadoIA.Exito)
                {
                    _logger.LogInformation($"Procesamiento IA exitoso para documento ID: {documento.Id}. Imágenes: {resultadoIA.NumeroImagenes}");

                    // 4. Actualizar documento con resultados del IA
                    await _documentoService.MarcarComoCompletadoAsync(
                        documento.Id,
                        resultadoIA.NumeroImagenes,
                        resultadoIA.Resumen,
                        resultadoIA.ContenidoExtraido,
                        resultadoIA.MetadataAdicionalJson
                    );

                    // 5. Actualizar ProcesoIA como completado
                    var resultadoJson = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        numeroImagenes = resultadoIA.NumeroImagenes,
                        resumen = resultadoIA.Resumen,
                        contenidoExtraido = resultadoIA.ContenidoExtraido,
                        metadata = resultadoIA.MetadataAdicional
                    });

                    await _procesoIAService.MarcarComoCompletadoAsync(procesoIA.Id, resultadoJson);

                    _logger.LogInformation($"Procesamiento completado para documento ID: {documento.Id}");
                }
                else
                {
                    _logger.LogError($"Error en procesamiento IA para documento ID: {documento.Id}: {resultadoIA.Error}");

                    // 6. Manejar error del servicio IA
                    await _documentoService.MarcarComoErrorAsync(documento.Id, resultadoIA.Error ?? "Error desconocido del servicio IA");
                    await _procesoIAService.MarcarComoErrorAsync(procesoIA.Id, resultadoIA.Error ?? "Error desconocido del servicio IA");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en procesamiento completo para documento ID: {documento.Id}: {ex.Message}");

                // 7. Manejar excepciones generales
                await _documentoService.MarcarComoErrorAsync(documento.Id, ex.Message);
                await _procesoIAService.MarcarComoErrorAsync(procesoIA.Id, ex.Message);
            }
        }

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
                await _documentoService.EliminarAsync(id);
                return Ok(new { mensaje = "Documento eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar documento {id}");
                return StatusCode(500, new { mensaje = "Error al eliminar documento", error = ex.Message });
            }
        }
    }
}