using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Application.DTOs.Insight;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightController : ControllerBase
    {
        private readonly InsightService _insightService;
        private readonly ILogger<InsightController> _logger;

        public InsightController(
            InsightService insightService,
            ILogger<InsightController> logger)
        {
            _insightService = insightService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var insights = await _insightService.ObtenerTodosAsync();
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los insights");
                return StatusCode(500, new { mensaje = "Error interno al obtener insights", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var insight = await _insightService.ObtenerPorIdAsync(id);
                if (insight == null)
                    return NotFound(new { mensaje = "Insight no encontrado" });

                return Ok(insight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener insight {id}");
                return StatusCode(500, new { mensaje = "Error al obtener insight", error = ex.Message });
            }
        }

        [HttpGet("documento/{documentoId}")]
        public async Task<IActionResult> GetByDocumento(int documentoId)
        {
            try
            {
                var insights = await _insightService.ObtenerPorDocumentoIdAsync(documentoId);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener insights del documento {documentoId}");
                return StatusCode(500, new { mensaje = "Error al obtener insights del documento", error = ex.Message });
            }
        }

        [HttpGet("proceso-ia/{procesoIAId}")]
        public async Task<IActionResult> GetByProcesoIA(int procesoIAId)
        {
            try
            {
                var insights = await _insightService.ObtenerPorProcesoIAIdAsync(procesoIAId);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener insights del proceso IA {procesoIAId}");
                return StatusCode(500, new { mensaje = "Error al obtener insights del proceso IA", error = ex.Message });
            }
        }

        [HttpGet("tipo/{tipo}")]
        public async Task<IActionResult> GetByTipo(string tipo)
        {
            try
            {
                var insights = await _insightService.ObtenerPorTipoAsync(tipo);
                return Ok(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener insights de tipo {tipo}");
                return StatusCode(500, new { mensaje = "Error al obtener insights por tipo", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InsightCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState.Values.SelectMany(v => v.Errors) });

                var insight = await _insightService.CrearInsightAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = insight.Id }, insight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear insight");
                return StatusCode(500, new { mensaje = "Error interno al crear insight", error = ex.Message });
            }
        }

        [HttpPost("generar-desde-rag/{documentoId}")]
        public async Task<IActionResult> GenerarInsightDesdeRAG(int documentoId, [FromBody] GenerarInsightRequest request)
        {
            try
            {
                var insight = await _insightService.GenerarInsightDesdeRAGAsync(documentoId, request.Pregunta, request.TipoInsight);
                return Ok(new { mensaje = "Insight generado exitosamente", insight });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar insight desde RAG para documento {documentoId}");
                return StatusCode(500, new { mensaje = "Error al generar insight desde RAG", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] InsightUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { mensaje = "Datos inválidos", errores = ModelState.Values.SelectMany(v => v.Errors) });

                var insight = await _insightService.ActualizarInsightAsync(id, dto);
                if (insight == null)
                    return NotFound(new { mensaje = "Insight no encontrado" });

                return Ok(insight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar insight {id}");
                return StatusCode(500, new { mensaje = "Error al actualizar insight", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _insightService.EliminarInsightAsync(id);
                if (!resultado)
                    return NotFound(new { mensaje = "Insight no encontrado" });

                return Ok(new { mensaje = "Insight eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar insight {id}");
                return StatusCode(500, new { mensaje = "Error al eliminar insight", error = ex.Message });
            }
        }
    }

    public class GenerarInsightRequest
    {
        public string Pregunta { get; set; } = string.Empty;
        public string TipoInsight { get; set; } = "analisis";
    }
}