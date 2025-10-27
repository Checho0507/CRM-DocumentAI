using Microsoft.AspNetCore.Mvc;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Application.Services;

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

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile archivo, [FromForm] int clienteId)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Archivo no válido");

            // Convertir archivo a bytes para almacenarlo en la BD
            byte[] archivoBytes;
            using (var memoryStream = new MemoryStream())
            {
                await archivo.CopyToAsync(memoryStream);
                archivoBytes = memoryStream.ToArray();
            }

            // Crear carpeta si no existe (opcional si también quieres guardarlo físico)
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, archivo.FileName);
            await System.IO.File.WriteAllBytesAsync(filePath, archivoBytes);

            // Crear objeto Documento con toda la info
            var documento = new Documento
            {
                ClienteId = clienteId,
                NombreArchivo = archivo.FileName,
                TipoDocumento = Path.GetExtension(archivo.FileName),
                RutaArchivo = filePath,
                FechaSubida = DateTime.UtcNow,
                Procesado = false,
                ArchivoDocumento = archivoBytes, // ✅ Importante
                ArchivoMetadataJson = null       // Por ahora sin metadatos
            };

            await _documentoService.AgregarAsync(documento);

            return Ok(new
            {
                mensaje = "Archivo subido correctamente",
                documentoId = documento.Id,
                ruta = filePath
            });
        }

    }
}
