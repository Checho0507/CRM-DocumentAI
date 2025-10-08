// Controllers/AuthController.cs

using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Application.DTOs.Auth;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Application.Services;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/auth")] // La ruta base que espera NextAuth
    public class AuthController : ControllerBase
    {
        private readonly AutenticacionService _servicioAuth;

        public AuthController(AutenticacionService servicioAuth)
        {
            _servicioAuth = servicioAuth;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registrar([FromBody] RegistroDTO dto)
        {
            try
            {
                await _servicioAuth.RegistrarUsuarioAsync(dto);
                return Ok(new { success = true, message = "Registro exitoso." });
            }
            catch (ArgumentException ex)
            {
                // Usuario ya existe
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Otro error (ej. validación del Email ValueObject)
                return BadRequest(new { success = false, message = "Error al registrar: " + ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAuthDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                var respuesta = await _servicioAuth.LoginAsync(dto);
                // NextAuth espera un objeto que contenga el token y datos de usuario
                return Ok(respuesta);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "Error interno al iniciar sesión." });
            }
        }

        // Endpoint para el flujo 'checkUser' de NextAuth (ej. después de Google OAuth)
        // Podría ser GET o POST dependiendo de cómo lo llames desde NextAuth.
        [HttpPost("check-user")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioInfoDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CheckUser([FromBody] LoginDTO dto)
        {
            var usuarioInfo = await _servicioAuth.ValidarUsuarioExternoAsync(dto.Email);

            if (usuarioInfo == null)
            {
                // Si no existe, NextAuth debe redirigir al registro o crear la cuenta.
                return NotFound(new { success = false, message = "Usuario no encontrado, debe registrarse." });
            }

            // Si el usuario existe, devolvemos sus datos
            return Ok(usuarioInfo);
        }
    }
}