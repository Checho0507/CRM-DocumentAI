using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class DocumentoService
    {
        private readonly IDocumentoRepository _documentoRepository;

        public DocumentoService(IDocumentoRepository documentoRepository)
        {
            _documentoRepository = documentoRepository;
        }

        public Task<IEnumerable<Documento>> ObtenerTodosAsync() 
            => _documentoRepository.ObtenerTodosAsync();

        public Task<Documento?> ObtenerPorIdAsync(int id) 
            => _documentoRepository.ObtenerPorIdAsync(id);

        public Task<IEnumerable<Documento>> ObtenerPorUsuarioIdAsync(int usuarioId) 
            => _documentoRepository.ObtenerPorUsuarioIdAsync(usuarioId);

        public Task<IEnumerable<Documento>> ObtenerPorEstadoAsync(string estado) 
            => _documentoRepository.ObtenerPorEstadoAsync(estado);

        public Task AgregarAsync(Documento documento) 
            => _documentoRepository.AgregarAsync(documento);

        public Task ActualizarAsync(Documento documento) 
            => _documentoRepository.ActualizarAsync(documento);

        public Task EliminarAsync(int id) 
            => _documentoRepository.EliminarAsync(id);

        // 🔹 MÉTODOS NUEVOS - Agregados

        public async Task ActualizarEstadoAsync(int id, string estado, string? mensajeError = null)
        {
            var documento = await _documentoRepository.ObtenerPorIdAsync(id);
            if (documento == null)
                throw new KeyNotFoundException($"Documento con ID {id} no encontrado");

            documento.EstadoProcesamiento = estado;
            documento.ErrorProcesamiento = mensajeError;
            
            await _documentoRepository.ActualizarAsync(documento);
        }

        public async Task<object> ObtenerEstadisticasPorUsuarioAsync(int usuarioId)
        {
            var documentos = (await _documentoRepository.ObtenerPorUsuarioIdAsync(usuarioId)).ToList();

            return new
            {
                TotalDocumentos = documentos.Count,
                Completados = documentos.Count(d => d.EstadoProcesamiento == "completado"),
                Procesando = documentos.Count(d => d.EstadoProcesamiento == "procesando"),
                ConError = documentos.Count(d => d.EstadoProcesamiento == "error"),
                Pendientes = documentos.Count(d => d.EstadoProcesamiento == "pendiente"),
                UltimaActualizacion = DateTime.UtcNow
            };
        }

        // Métodos específicos de negocio
        public async Task<bool> MarcarComoProcesandoAsync(int documentoId)
        {
            var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
            if (documento == null) return false;

            documento.EstadoProcesamiento = "procesando";
            await _documentoRepository.ActualizarAsync(documento);
            return true;
        }

        public async Task<bool> MarcarComoCompletadoAsync(int documentoId, int numeroImagenes, string resumen, string? metadataJson = null)
        {
            var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
            if (documento == null) return false;

            documento.EstadoProcesamiento = "completado";
            documento.Procesado = true;
            documento.NumeroImagenes = numeroImagenes;
            documento.ResumenDocumento = resumen;
            documento.FechaProcesamiento = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(metadataJson))
            {
                documento.ArchivoMetadataJson = metadataJson;
            }

            await _documentoRepository.ActualizarAsync(documento);
            return true;
        }

        public async Task<bool> MarcarComoErrorAsync(int documentoId, string error)
        {
            var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
            if (documento == null) return false;

            documento.EstadoProcesamiento = "error";
            documento.ErrorProcesamiento = error;
            await _documentoRepository.ActualizarAsync(documento);
            return true;
        }

        // Método para subir documento con procesamiento inicial
        public async Task<Documento> SubirDocumentoAsync(IFormFile archivo, int usuarioId, IWebHostEnvironment environment)
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("El archivo no es válido");

            // Convertir archivo a bytes
            byte[] archivoBytes;
            using (var memoryStream = new MemoryStream())
            {
                await archivo.CopyToAsync(memoryStream);
                archivoBytes = memoryStream.ToArray();
            }

            // Crear carpeta de uploads si no existe
            var uploadsFolder = Path.Combine(environment.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generar nombre único para el archivo
            var fileName = $"{Guid.NewGuid()}_{archivo.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Guardar archivo físicamente
            await System.IO.File.WriteAllBytesAsync(filePath, archivoBytes);

            // Crear entidad Documento
            var documento = new Documento
            {
                UsuarioId = usuarioId,
                NombreArchivo = archivo.FileName,
                TipoDocumento = Path.GetExtension(archivo.FileName),
                RutaArchivo = filePath,
                FechaSubida = DateTime.UtcNow,
                Procesado = false,
                ArchivoDocumento = archivoBytes,
                TamañoArchivo = archivo.Length,
                EstadoProcesamiento = "pendiente"
            };

            await _documentoRepository.AgregarAsync(documento);
            return documento;
        }
    }
}