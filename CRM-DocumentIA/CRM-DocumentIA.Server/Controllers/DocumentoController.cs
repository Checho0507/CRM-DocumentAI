using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly DocumentoService _documentoService;

        public DocumentoController(DocumentoService documentoService)
        {
            _documentoService = documentoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _documentoService.ObtenerTodosAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var documento = await _documentoService.ObtenerPorIdAsync(id);
            return documento == null ? NotFound() : Ok(documento);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Documento documento)
        {
            await _documentoService.AgregarAsync(documento);
            return CreatedAtAction(nameof(Get), new { id = documento.Id }, documento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Documento documento)
        {
            if (id != documento.Id) return BadRequest();
            await _documentoService.ActualizarAsync(documento);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _documentoService.EliminarAsync(id);
            return NoContent();
        }
    }
}
