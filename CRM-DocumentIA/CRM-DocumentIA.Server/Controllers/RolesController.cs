using CRM_DocumentIA.Server.Application.DTOs.Rol;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CRM_DocumentIA.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Puedes exigir que solo admin acceda
    [Authorize(Roles = "admin")]
    public class RolesController : ControllerBase
    {
        private readonly RolService _rolService;

        public RolesController(RolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _rolService.ObtenerTodosAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRol(int id)
        {
            var rol = await _rolService.ObtenerPorIdAsync(id);
            if (rol == null) return NotFound();
            return Ok(rol);
        }

        [HttpPost]
        public async Task<IActionResult> CrearRol([FromBody] CrearRolDTO dto)
        {
            var rol = await _rolService.CrearAsync(dto);
            return CreatedAtAction(nameof(GetRol), new { id = rol.Id }, rol);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarRol(int id, [FromBody] ActualizarRolDTO dto)
        {
            var rol = await _rolService.ActualizarAsync(id, dto);
            if (rol == null) return NotFound();
            return Ok(rol);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarRol(int id)
        {
            var eliminado = await _rolService.EliminarAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }
    }
}
