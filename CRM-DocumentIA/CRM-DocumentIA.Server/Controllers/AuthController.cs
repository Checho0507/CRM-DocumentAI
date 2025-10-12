// Controllers/AuthController.cs

using System.Collections.Concurrent;
using CRM_DocumentIA.Application.Services;
using CRM_DocumentIA.Server.Application.DTOs._2FA;
using CRM_DocumentIA.Server.Application.DTOs.Auth;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Mvc;

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

        private class TwoFaEntry
        {
            public string Code { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
            public int Attempts { get; set; }
        }

        // Esto vive en memoria mientras corre la app:
        private static readonly ConcurrentDictionary<string, TwoFaEntry> _twoFaStore = new();

        private readonly SmtpEmailService _smtpEmailService;

        // Ajusta si tu constructor ya existe: añade SmtpEmailService como parámetro e asigna
        public AuthController(SmtpEmailService smtpEmailService /*, otros services... */)
        {
            _smtpEmailService = smtpEmailService;
            // asignar otros services si ya los tienes...
        }

        // POST: api/auth/send-2fa-code
        [HttpPost("send-2fa-code")]
        public async Task<IActionResult> Send2FaCode([FromBody] Envio2FADTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "El email es requerido." });

            // generar código 6 dígitos
            var rng = new Random();
            var code = rng.Next(0, 1000000).ToString("D6");

            // guardar en memoria con expiración (5 minutos)
            var entry = new TwoFaEntry
            {
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0
            };
            _twoFaStore[dto.Email.ToLowerInvariant()] = entry;

            // enviar correo (HTML simple)
            var subject = "Código de verificación - CRM DocumentIA";
            var body = $"<p>Tu código de verificación es: <strong>{code}</strong></p><p>Expira en 5 minutos.</p>";

            try
            {
                await _smtpEmailService.SendEmailAsync(dto.Email, subject, body);
                return Ok(new { message = "Código enviado." });
            }
            catch (Exception ex)
            {
                // si falla el envío, remueve el código guardado
                _twoFaStore.TryRemove(dto.Email.ToLowerInvariant(), out _);
                // log ex si quieres
                return StatusCode(500, new { message = "No se pudo enviar el correo.", detail = ex.Message });
            }
        }

        // POST: api/auth/verify-2fa-code
        [HttpPost("verify-2fa-code")]
        public IActionResult Verify2FaCode([FromBody] Verificacion2FADTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Codigo))
                return BadRequest(new { message = "Email y código son requeridos." });

            var key = dto.Email.ToLowerInvariant();
            if (!_twoFaStore.TryGetValue(key, out var entry))
                return BadRequest(new { message = "No hay un código pendiente para este email." });

            // revisar expiración
            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                _twoFaStore.TryRemove(key, out _);
                return BadRequest(new { message = "El código ha expirado." });
            }

            // revisar intentos
            if (entry.Attempts >= 5)
            {
                _twoFaStore.TryRemove(key, out _);
                return BadRequest(new { message = "Se ha excedido el número de intentos. Solicita un nuevo código." });
            }

            // comparar código
            if (entry.Code != dto.Codigo)
            {
                entry.Attempts++;
                _twoFaStore[key] = entry;
                return BadRequest(new { message = "Código inválido." });
            }

            // correcto: eliminar y devolver success
            _twoFaStore.TryRemove(key, out _);
            return Ok(new { message = "Código verificado." });
        }

    }

}