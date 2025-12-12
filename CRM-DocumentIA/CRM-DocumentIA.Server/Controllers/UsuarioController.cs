// Controllers/UsuarioController.cs
using CRM_DocumentIA.Server.Application.DTOs.Usuario;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(IUsuarioService usuarioService, ILogger<UsuarioController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet("perfil/{usuarioId}")]
        public async Task<IActionResult> ObtenerPerfil(int usuarioId)
        {
            try
            {
                var usuario = await _usuarioService.ObtenerPerfilAsync(usuarioId);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(new UsuarioDTO
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email?.Value ?? string.Empty,
                    RolId = usuario.RolId,
                    RolNombre = usuario.Rol?.Nombre ?? "Sin rol",
                    DobleFactorActivado = usuario.DobleFactorActivado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, new { mensaje = "Error al obtener perfil", error = ex.Message });
            }
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarUsuarios([FromQuery] string search)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(search))
                    return Ok(new List<UsuarioDTO>());

                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var usuarios = await _usuarioService.BuscarUsuariosAsync(search, usuarioId);

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuarios con término: {SearchTerm}", search);
                return StatusCode(500, new { mensaje = "Error al buscar usuarios", error = ex.Message });
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
                usuario.Nombre = dto.Nombre ?? usuario.Nombre;

                await _usuarioService.ActualizarPerfilAsync(usuario);

                return Ok(new { mensaje = "Perfil actualizado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil del usuario {UsuarioId}", usuarioId);
                return StatusCode(500, new { mensaje = "Error al actualizar perfil", error = ex.Message });
            }
        }

        [HttpPost("asignar-rol")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AsignarRol([FromBody] AsignarRolDTO dto)
        {
            try
            {
                var resultado = await _usuarioService.AsignarRolAsync(dto.UsuarioId, dto.RolId);

                if (!resultado)
                    return BadRequest(new { mensaje = "No se pudo asignar el rol. Verifique el usuario y el rol." });

                return Ok(new { mensaje = "Rol asignado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar rol {RolId} al usuario {UsuarioId}", dto.RolId, dto.UsuarioId);
                return StatusCode(500, new { mensaje = "Error al asignar rol", error = ex.Message });
            }
        }

        [HttpGet("actual")]
        public async Task<IActionResult> ObtenerUsuarioActual()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId <= 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var usuario = await _usuarioService.ObtenerPorIdAsync(usuarioId);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(new UsuarioDTO
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email?.Value ?? string.Empty,
                    RolId = usuario.RolId,
                    RolNombre = usuario.Rol?.Nombre ?? "Sin rol",
                    DobleFactorActivado = usuario.DobleFactorActivado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new { mensaje = "Error al obtener usuario actual", error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ObtenerTodosUsuarios()
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerTodosAsync();
                var resultado = usuarios.Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email?.Value ?? string.Empty,
                    RolId = u.RolId,
                    RolNombre = u.Rol?.Nombre ?? "Sin rol",
                    DobleFactorActivado = u.DobleFactorActivado
                });

                return Ok(new
                {
                    total = resultado.Count(),
                    usuarios = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode(500, new { mensaje = "Error al obtener usuarios", error = ex.Message });
            }
        }
        [HttpGet("buscar-por-email")]
        public async Task<IActionResult> BuscarUsuariosPorEmail([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { mensaje = "El email no puede estar vacío" });

                var usuario = await _usuarioService.ObtenerPorEmailAsync(email);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(new UsuarioDTO
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email?.Value ?? string.Empty,
                    RolId = usuario.RolId,
                    RolNombre = usuario.Rol?.Nombre ?? "Sin rol",
                    DobleFactorActivado = usuario.DobleFactorActivado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar usuario por email: {Email}", email);
                return StatusCode(500, new { mensaje = "Error al buscar usuario", error = ex.Message });
            }
        }

        // Eliminado el endpoint ObtenerUsuariosActivos ya que la entidad Usuario no tiene propiedad Activo

        // Eliminado el endpoint ObtenerEstadisticas ya que usaba métodos no implementados y la propiedad Activo

        // Endpoint adicional para verificar si el usuario actual existe y tiene permisos
        [HttpGet("verificar")]
        public IActionResult VerificarUsuario()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (usuarioId <= 0)
                    return Unauthorized(new { mensaje = "Usuario no autenticado" });

                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var nombre = User.FindFirst(ClaimTypes.Name)?.Value;

                return Ok(new
                {
                    usuarioId,
                    nombre,
                    email,
                    autenticado = true,
                    fecha = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar usuario");
                return StatusCode(500, new { mensaje = "Error al verificar usuario", error = ex.Message });
            }
        }

        // Endpoint para obtener conteo básico de usuarios (solo admin)
        [HttpGet("conteo")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ObtenerConteoUsuarios()
        {
            try
            {
                var usuarios = await _usuarioService.ObtenerTodosAsync();
                var totalUsuarios = usuarios.Count();

                // Conteo por rol
                var conteoPorRol = usuarios
                    .GroupBy(u => u.Rol?.Nombre ?? "Sin rol")
                    .ToDictionary(g => g.Key, g => g.Count());

                // Conteo por estado de doble factor
                var conDobleFactor = usuarios.Count(u => u.DobleFactorActivado);
                var sinDobleFactor = totalUsuarios - conDobleFactor;

                return Ok(new
                {
                    total = totalUsuarios,
                    porRol = conteoPorRol,
                    dobleFactor = new
                    {
                        activado = conDobleFactor,
                        desactivado = sinDobleFactor
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de usuarios");
                return StatusCode(500, new { mensaje = "Error al obtener conteo de usuarios", error = ex.Message });
            }
        }
    }
}