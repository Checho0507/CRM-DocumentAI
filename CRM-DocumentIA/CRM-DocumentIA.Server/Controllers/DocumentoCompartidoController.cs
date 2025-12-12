using CRM_DocumentIA.Server.Application.DTOs.Documento;
using CRM_DocumentIA.Server.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentoCompartidoController : ControllerBase
    {
        private readonly DocumentoCompartidoService _documentoCompartidoService;
        private readonly ILogger<DocumentoCompartidoController> _logger;

        public DocumentoCompartidoController(
            DocumentoCompartidoService documentoCompartidoService,
            ILogger<DocumentoCompartidoController> logger)
        {
            _documentoCompartidoService = documentoCompartidoService;
            _logger = logger;
        }

        // 🔥 Método SEGURO para obtener el usuarioId desde el token
        private int GetUsuarioId()
        {
            var usuarioIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("id")?.Value ??
                User.FindFirst("userId")?.Value;

            if (usuarioIdClaim == null)
                throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario desde el token.");

            return int.Parse(usuarioIdClaim);
        }

        // POST: Compartir documento
        [HttpPost]
        public async Task<IActionResult> CompartirDocumento([FromBody] CompartirDocumentoDTO dto)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var resultado = await _documentoCompartidoService.CompartirDocumentoAsync(dto, usuarioId);

                _logger.LogInformation($"✅ Documento {dto.DocumentoId} compartido con usuario {dto.UsuarioCompartidoId}");

                return Ok(new
                {
                    mensaje = "Documento compartido correctamente",
                    compartidoId = resultado.Id,
                    documentoId = resultado.DocumentoId,
                    usuarioCompartidoId = resultado.UsuarioCompartidoId,
                    fecha = resultado.FechaCompartido
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "❌ Intento no autorizado de compartir documento");
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al compartir documento");
                return StatusCode(500, new { mensaje = "Error al compartir documento", error = ex.Message });
            }
        }

        // GET: Documentos compartidos CONMIGO
        [HttpGet("conmigo")]
        public async Task<IActionResult> GetDocumentosCompartidosConmigo()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var documentos = await _documentoCompartidoService.ObtenerDocumentosCompartidosConmigoAsync(usuarioId);

                return Ok(new
                {
                    documentos = documentos.Select(d => new
                    {
                        id = d.Id,
                        documentoId = d.DocumentoId,
                        nombreDocumento = d.Documento?.NombreArchivo ?? "Documento no encontrado",
                        tipoDocumento = d.Documento?.TipoDocumento ?? "Desconocido",
                        tamañoDocumento = d.Documento?.TamañoArchivo,
                        fechaSubida = d.Documento?.FechaSubida ?? DateTime.MinValue,
                        compartidoPor = d.UsuarioPropietario?.Nombre ?? "Usuario desconocido",
                        emailCompartidoPor = d.UsuarioPropietario?.Email ?? "",
                        fechaCompartido = d.FechaCompartido,
                        permiso = d.Permiso,
                        mensaje = d.Mensaje,
                        estadoProcesamiento = d.Documento?.EstadoProcesamiento ?? "pendiente",
                        procesado = d.Documento?.Procesado ?? false
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documentos compartidos conmigo");
                return StatusCode(500, new { mensaje = "Error al obtener documentos compartidos conmigo", error = ex.Message });
            }
        }

        // GET: Documentos compartidos POR MI
        [HttpGet("mios")]
        public async Task<IActionResult> GetDocumentosQueHeCompartido()
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var documentos = await _documentoCompartidoService.ObtenerDocumentosQueHeCompartidoAsync(usuarioId);

                return Ok(new
                {
                    documentos = documentos.Select(d => new
                    {
                        id = d.Id,
                        nombreDocumento = d.Documento?.NombreArchivo ?? "Documento no encontrado",
                        nombreUsuarioCompartido = d.UsuarioCompartido?.Nombre ?? "Usuario desconocido",
                        email = d.UsuarioCompartido?.Email ?? "",
                        fechaCompartido = d.FechaCompartido,
                        permiso = d.Permiso,
                        mensaje = d.Mensaje
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener documentos compartidos por mí");
                return StatusCode(500, new { mensaje = "Error al obtener documentos compartidos por mí", error = ex.Message });
            }
        }

        // GET: Usuarios con los que se compartió un documento
        [HttpGet("documento/{documentoId}")]
        public async Task<IActionResult> ObtenerUsuariosCompartidos(int documentoId)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var usuarios = await _documentoCompartidoService.ObtenerUsuariosCompartidosAsync(documentoId, usuarioId);

                return Ok(new
                {
                    total = usuarios.Count(),
                    usuarios = usuarios.Select(u => new
                    {
                        id = u.Id,
                        nombre = u.Nombre,
                        email = u.Email,
                        rol = u.Rol
                    })
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al obtener usuarios compartidos del documento {documentoId}");
                return StatusCode(500, new { mensaje = "Error al obtener usuarios compartidos", error = ex.Message });
            }
        }

        // DELETE: Dejar de compartir documento
        [HttpDelete("{id}")]
        public async Task<IActionResult> DejarDeCompartir(int id)
        {
            try
            {
                var usuarioId = GetUsuarioId();
                var resultado = await _documentoCompartidoService.DejarDeCompartirAsync(id, usuarioId);

                if (!resultado)
                    return NotFound(new { mensaje = "Compartido no encontrado" });

                return Ok(new { mensaje = "Dejó de compartirse correctamente" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error al dejar de compartir {id}");
                return StatusCode(500, new { mensaje = "Error al dejar de compartir", error = ex.Message });
            }
        }

        // GET: Todos los compartidos (Admin)
        [HttpGet("todos")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerTodosLosCompartidos()
        {
            try
            {
                var compartidos = await _documentoCompartidoService.ObtenerTodosLosCompartidosAsync();

                return Ok(new
                {
                    total = compartidos.Count(),
                    compartidos = compartidos.Select(c => new
                    {
                        id = c.Id,
                        documentoId = c.DocumentoId,
                        nombreDocumento = c.Documento?.NombreArchivo ?? "Documento no encontrado",
                        usuarioPropietario = c.UsuarioPropietario?.Nombre,
                        usuarioCompartido = c.UsuarioCompartido?.Nombre,
                        fechaCompartido = c.FechaCompartido,
                        permiso = c.Permiso,
                        mensaje = c.Mensaje
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener todos los documentos compartidos");
                return StatusCode(500, new { mensaje = "Error al obtener documentos compartidos", error = ex.Message });
            }
        }
    }
}
