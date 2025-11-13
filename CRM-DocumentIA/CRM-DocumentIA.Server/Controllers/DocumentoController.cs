using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Application.DTOs.Documento;

namespace CRM_DocumentIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoController : ControllerBase
    {
        private readonly DocumentoService _documentoService;
        private readonly IWebHostEnvironment _environment;

        public DocumentoController(DocumentoService documentoService, IWebHostEnvironment environment)
        {
            _documentoService = documentoService;
            _environment = environment;
        }

        // ✅ SUBIR DOCUMENTO
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] DocumentoUploadDto dto)
        {
            try
            {
                if (dto?.Archivo == null || dto.Archivo.Length == 0)
                    return BadRequest(new { mensaje = "Archivo no válido o vacío" });

                if (dto.UsuarioId <= 0)
                    return BadRequest(new { mensaje = "UsuarioId debe ser mayor a 0" });

                // Validar extensión de archivo permitida
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(dto.Archivo.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { mensaje = "Tipo de archivo no permitido" });

                // Convertir archivo a bytes
                byte[] archivoBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await dto.Archivo.CopyToAsync(memoryStream);
                    archivoBytes = memoryStream.ToArray();
                }

                // Crear carpeta "Uploads" si no existe
                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generar nombre único para el archivo
                var fileName = $"{Guid.NewGuid()}_{dto.Archivo.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                await System.IO.File.WriteAllBytesAsync(filePath, archivoBytes);

                // Crear entidad Documento
                var documento = new Documento
                {
                    UsuarioId = dto.UsuarioId,
                    NombreArchivo = dto.Archivo.FileName,
                    TipoDocumento = extension,
                    RutaArchivo = filePath,
                    FechaSubida = DateTime.UtcNow,
                    Procesado = false,
                    ArchivoDocumento = archivoBytes,
                    ArchivoMetadataJson = null,
                    TamañoArchivo = dto.Archivo.Length
                };

                await _documentoService.AgregarAsync(documento);

                return Ok(new
                {
                    mensaje = "Archivo subido correctamente",
                    documentoId = documento.Id,
                    usuarioId = dto.UsuarioId,
                    nombreArchivo = dto.Archivo.FileName,
                    ruta = filePath,
                    tamaño = dto.Archivo.Length,
                    fechaSubida = documento.FechaSubida
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno del servidor al subir el archivo",
                    error = ex.Message
                });
            }
        }

        // ✅ OBTENER TODOS LOS DOCUMENTOS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var documentos = await _documentoService.ObtenerTodosAsync();
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener documentos", error = ex.Message });
            }
        }

        // ✅ OBTENER DOCUMENTO POR ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                return Ok(documento);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener documento", error = ex.Message });
            }
        }

        // ✅ OBTENER DOCUMENTOS POR USUARIO
        [HttpGet("usuario/{usuarioId}")]
        public async Task<IActionResult> GetByUsuario(int usuarioId)
        {
            try
            {
                var documentos = await _documentoService.ObtenerPorUsuarioIdAsync(usuarioId);
                return Ok(documentos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener documentos del usuario", error = ex.Message });
            }
        }

        // ✅ ELIMINAR DOCUMENTO (INCLUYE ELIMINAR ARCHIVO FÍSICO)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var documento = await _documentoService.ObtenerPorIdAsync(id);
                if (documento == null)
                    return NotFound(new { mensaje = "Documento no encontrado" });

                // Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(documento.RutaArchivo) && System.IO.File.Exists(documento.RutaArchivo))
                {
                    System.IO.File.Delete(documento.RutaArchivo);
                }

                await _documentoService.EliminarAsync(id);

                return Ok(new { mensaje = "Documento eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar documento", error = ex.Message });
            }
        }
    }
}
