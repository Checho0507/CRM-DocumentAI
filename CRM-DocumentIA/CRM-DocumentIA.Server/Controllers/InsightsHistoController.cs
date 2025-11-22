using CRM_DocumentIA.Server.Application.DTOs.InsightsHisto;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsHistoController : ControllerBase
    {
        private readonly InsightsHistoService _service;

        public InsightsHistoController(InsightsHistoService service)
        {
            _service = service;
        }

        // POST: api/insightsHisto
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInsightsHistoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        // GET: api/insightsHisto/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound("El registro no existe.");

            return Ok(result);
        }

        // GET: api/insightsHisto/user/{userId}
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }

        // GET: api/insightsHisto
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        // PUT: api/insightsHisto/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateInsightsHistoDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            if (!success)
                return NotFound("No se pudo actualizar el registro.");

            return Ok("Registro actualizado correctamente.");
        }

        // DELETE: api/insightsHisto/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound("No se pudo eliminar el registro.");

            return Ok("Registro eliminado correctamente.");
        }
    }
}
