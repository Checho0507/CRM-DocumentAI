// Controllers/UsuarioController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Application.DTOs.Usuario;
using System.Security.Claims;

namespace CRM_DocumentIA.Server.Controllers
{
    [Authorize] // Requiere que el usuario esté autenticado con JWT
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _servicioUsuario;

        public UsuarioController(UsuarioService servicioUsuario)
        {
            _servicioUsuario = servicioUsuario;
        }

        // Función auxiliar para obtener el ID del usuario del token JWT
        private int ObtenerUsuarioId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                // Esto no debería pasar si [Authorize] funciona correctamente
                throw new UnauthorizedAccessException("ID de usuario no encontrado en el token.");
            }
            return userId;
        }

        [HttpGet("perfil")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var userId = ObtenerUsuarioId();

            var perfil = await _servicioUsuario.ObtenerPerfilAsync(userId);

            if (perfil == null)
            {
                return NotFound(new { message = "Perfil no encontrado." });
            }

            return Ok(perfil);
        }

        [HttpPut("perfil")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizacionUsuarioDTO dto)
        {
            var userId = ObtenerUsuarioId();

            try
            {
                await _servicioUsuario.ActualizarPerfilAsync(userId, dto);
                return Ok(new { message = "Perfil actualizado con éxito." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error al actualizar: {ex.Message}" });
            }
        }
    }
}