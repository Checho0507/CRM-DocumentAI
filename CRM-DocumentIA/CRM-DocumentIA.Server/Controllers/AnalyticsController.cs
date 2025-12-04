using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Application.DTOs.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsService _analyticsService;

        public AnalyticsController(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Obtiene un resumen general del dashboard
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            var summary = await _analyticsService.GetSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Obtiene la cantidad de consultas por día
        /// </summary>
        [HttpGet("consultas-por-dia")]
        public async Task<ActionResult<IEnumerable<ConsultasPorDiaDto>>> GetConsultasPorDia()
        {
            var data = await _analyticsService.GetConsultasPorDiaAsync();
            return Ok(data);
        }

        /// <summary>
        /// Obtiene los top usuarios por cantidad de consultas
        /// </summary>
        [HttpGet("top-usuarios")]
        public async Task<ActionResult<IEnumerable<TopUsuariosDto>>> GetTopUsuarios()
        {
            var data = await _analyticsService.GetTopUsuariosAsync();
            return Ok(data);
        }
    }
}
