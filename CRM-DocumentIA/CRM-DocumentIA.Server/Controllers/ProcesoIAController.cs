using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Application.Services;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcesoIAController : ControllerBase
    {
        private readonly ProcesoIAService _procesoIAService;
        private readonly ILogger<ProcesoIAController> _logger;

        public ProcesoIAController(ProcesoIAService procesoIAService, ILogger<ProcesoIAController> logger)
        {
            _procesoIAService = procesoIAService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var procesos = await _procesoIAService.ObtenerTodosAsync();
                return Ok(procesos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los procesos IA");
                return StatusCode(500, new { mensaje = "Error al obtener procesos IA", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var proceso = await _procesoIAService.ObtenerPorIdAsync(id);
                if (proceso == null)
                    return NotFound(new { mensaje = "Proceso IA no encontrado" });

                return Ok(proceso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener proceso IA {id}");
                return StatusCode(500, new { mensaje = "Error al obtener proceso IA", error = ex.Message });
            }
        }

        [HttpGet("documento/{documentoId}")]
        public async Task<IActionResult> GetByDocumento(int documentoId)
        {
            try
            {
                var procesos = await _procesoIAService.ObtenerPorDocumentoIdAsync(documentoId);
                return Ok(procesos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener procesos IA del documento {documentoId}");
                return StatusCode(500, new { mensaje = "Error al obtener procesos IA del documento", error = ex.Message });
            }
        }

        [HttpGet("estado/{estado}")]
        public async Task<IActionResult> GetByEstado(string estado)
        {
            try
            {
                var procesos = await _procesoIAService.ObtenerPorEstadoAsync(estado);
                return Ok(procesos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener procesos IA con estado {estado}");
                return StatusCode(500, new { mensaje = "Error al obtener procesos IA por estado", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProcesoIADto dto)
        {
            try
            {
                if (dto.DocumentoId <= 0)
                    return BadRequest(new { mensaje = "DocumentoId debe ser mayor a 0" });

                var proceso = new ProcesoIA
                {
                    DocumentoId = dto.DocumentoId,
                    TipoProcesamiento = dto.TipoProcesamiento ?? "analisis_documento",
                    Estado = "pendiente",
                    FechaInicio = DateTime.UtcNow,
                    UrlServicio = dto.UrlServicio
                };

                await _procesoIAService.AgregarAsync(proceso);

                return Ok(new
                {
                    mensaje = "Proceso IA creado correctamente",
                    procesoId = proceso.Id,
                    documentoId = proceso.DocumentoId,
                    estado = proceso.Estado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al crear proceso IA para documento {dto.DocumentoId}");
                return StatusCode(500, new { mensaje = "Error al crear proceso IA", error = ex.Message });
            }
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoProcesoIADto dto)
        {
            try
            {
                var proceso = await _procesoIAService.ObtenerPorIdAsync(id);
                if (proceso == null)
                    return NotFound(new { mensaje = "Proceso IA no encontrado" });

                switch (dto.Estado.ToLower())
                {
                    case "procesando":
                        await _procesoIAService.MarcarComoProcesandoAsync(id);
                        break;
                    case "completado":
                        if (string.IsNullOrEmpty(dto.ResultadoJson))
                            return BadRequest(new { mensaje = "ResultadoJson es requerido para estado completado" });
                        
                        await _procesoIAService.MarcarComoCompletadoAsync(id, dto.ResultadoJson, dto.TiempoProcesamiento);
                        break;
                    case "error":
                        if (string.IsNullOrEmpty(dto.Error))
                            return BadRequest(new { mensaje = "Error es requerido para estado error" });
                        
                        await _procesoIAService.MarcarComoErrorAsync(id, dto.Error);
                        break;
                    default:
                        return BadRequest(new { mensaje = "Estado no válido" });
                }

                return Ok(new { mensaje = "Estado del proceso IA actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar estado del proceso IA {id}");
                return StatusCode(500, new { mensaje = "Error al actualizar estado del proceso IA", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _procesoIAService.EliminarAsync(id);
                return Ok(new { mensaje = "Proceso IA eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar proceso IA {id}");
                return StatusCode(500, new { mensaje = "Error al eliminar proceso IA", error = ex.Message });
            }
        }
    }

    // DTOs para el controller
    public class CreateProcesoIADto
    {
        public int DocumentoId { get; set; }
        public string? TipoProcesamiento { get; set; }
        public string? UrlServicio { get; set; }
    }

    public class UpdateEstadoProcesoIADto
    {
        public string Estado { get; set; } = string.Empty;
        public string? ResultadoJson { get; set; }
        public string? Error { get; set; }
        public double? TiempoProcesamiento { get; set; }
    }
}