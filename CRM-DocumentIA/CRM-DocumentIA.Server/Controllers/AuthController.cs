using System.Collections.Concurrent;
using CRM_DocumentIA.Server.Application.Services; // ✅ Corregido
using CRM_DocumentIA.Server.Infrastructure.Repositories;
using CRM_DocumentIA.Server.Application.DTOs._2FA;
using CRM_DocumentIA.Server.Application.DTOs.Auth;
using CRM_DocumentIA.Server.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AutenticacionService _servicioAuth;
        private readonly SmtpEmailService _smtpEmailService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly JWTService _jwtService;
        private static readonly ConcurrentDictionary<string, TwoFaEntry> _twoFaStore = new();

        public AuthController(
            AutenticacionService servicioAuth,
            SmtpEmailService smtpEmailService,
            JWTService jwtService,
            IUsuarioRepository usuarioRepository
        )
        {
            _servicioAuth = servicioAuth;
            _smtpEmailService = smtpEmailService;
            _usuarioRepository = usuarioRepository;
            _jwtService = jwtService;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        // 🎯 ENDPOINT: Login Social (Llamado 'SOCIAL_LOGIN' en el frontend)
        [HttpPost("social-login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RespuestaAuthDTO))]
        public async Task<IActionResult> SocialLogin([FromBody] LoginSocialDTO dto)
        {
            try
            {
                var respuesta = await _servicioAuth.LoginSocialAsync(dto);
                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = $"Error en el login social: {ex.Message}" });
            }
        }

        private class TwoFaEntry
        {
            public string Code { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public int Attempts { get; set; }
        }

        [HttpPost("send-2fa-code")]
        public async Task<IActionResult> Send2FaCode([FromBody] Envio2FADTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "El email es requerido." });

            var rng = new Random();
            var code = rng.Next(0, 1000000).ToString("D6");

            var entry = new TwoFaEntry
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0
            };
            _twoFaStore[dto.Email.ToLowerInvariant()] = entry;

            try
            {
                await _servicioAuth.EnviarCodigo2FAAsync(dto.Email, code);
                return Ok(new { message = "Código enviado." });
            }
            catch (Exception ex)
            {
                _twoFaStore.TryRemove(dto.Email.ToLowerInvariant(), out _);
                return StatusCode(500, new { message = "No se pudo enviar el correo.", detail = ex.Message });
            }
        }

        [HttpPost("verify-2fa-code")]
        public async Task<IActionResult> Verify2FaCode([FromBody] Verificacion2FADTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest(new { message = "Email y código son requeridos." });

            var key = dto.Email.ToLowerInvariant();
            if (!_twoFaStore.TryGetValue(key, out var entry))
                return BadRequest(new { message = "No hay un código pendiente para este email." });

            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                _twoFaStore.TryRemove(key, out _);
                return BadRequest(new { message = "El código ha expirado." });
            }

            if (entry.Attempts >= 5)
            {
                _twoFaStore.TryRemove(key, out _);
                return BadRequest(new { message = "Se ha excedido el número de intentos. Solicita un nuevo código." });
            }

            if (entry.Code != dto.Codigo)
            {
                entry.Attempts++;
                _twoFaStore[key] = entry;
                return BadRequest(new { message = "Código inválido." });
            }

            // ✅ Código correcto → autenticamos
            _twoFaStore.TryRemove(key, out _);

            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuario == null) return Unauthorized(new { message = "Usuario no encontrado." });

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                message = "Código verificado.",
                token,
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    Email = usuario.Email.Value, // ✅ Cambiado de .Valor a .Value
                    usuario.Rol
                }
            });
        }
    }
}