using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuarioController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("perfil/{usuarioId}")]
        public async Task<IActionResult> ObtenerPerfil(int usuarioId)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerPerfilAsync(usuarioId); // ✅ Este método ahora existe
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(new
                {
                    usuario.Id,
                    usuario.Nombre,
                    Email = usuario.Email.Value,
                    usuario.Rol
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener perfil", error = ex.Message });
            }
        }

        [HttpPut("perfil/{usuarioId}")]
        public async Task<IActionResult> ActualizarPerfil(int usuarioId, [FromBody] ActualizarPerfilDTO dto)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerPorIdAsync(usuarioId);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                // Actualizar propiedades
                usuario.Nombre = dto.Nombre;
                // No actualizamos Email porque es un Value Object y requiere validación

                await _usuarioService.ActualizarPerfilAsync(usuario); // ✅ Este método ahora existe

                return Ok(new { mensaje = "Perfil actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar perfil", error = ex.Message });
            }
        }

        // ... otros métodos del controller
    }

    // ✅ DTO para actualizar perfil (si no existe)
    public class ActualizarPerfilDTO
    {
        public string Nombre { get; set; } = string.Empty;
    }
}