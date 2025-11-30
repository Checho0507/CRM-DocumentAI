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

        // 🔹 MÉTODOS NUEVOS - Actualizados para la nueva lógica

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

        // Métodos específicos de negocio (mantenidos para compatibilidad)
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

        // 🔹 MÉTODOS NUEVOS PARA LA NUEVA LÓGICA

        // Método para subir documento básico (sin procesamiento)
        public async Task<Documento> SubirDocumentoBasicoAsync(IFormFile archivo, int usuarioId, IWebHostEnvironment environment)
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

            // Crear entidad Documento con estado pendiente
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

        // Método para guardar documento ya procesado con resultados del RAG
        public async Task<Documento> GuardarDocumentoProcesadoAsync(
            int documentoId, 
            int numeroImagenes, 
            string resumen, 
            string? metadataJson = null,
            string? urlServicioIA = null)
        {
            var documento = await _documentoRepository.ObtenerPorIdAsync(documentoId);
            if (documento == null)
                throw new KeyNotFoundException($"Documento con ID {documentoId} no encontrado");

            documento.EstadoProcesamiento = "completado";
            documento.Procesado = true;
            documento.NumeroImagenes = numeroImagenes;
            documento.ResumenDocumento = resumen;
            documento.FechaProcesamiento = DateTime.UtcNow;
            documento.UrlServicioIA = urlServicioIA;
            
            if (!string.IsNullOrEmpty(metadataJson))
            {
                documento.ArchivoMetadataJson = metadataJson;
            }

            await _documentoRepository.ActualizarAsync(documento);
            return documento;
        }

        // Método para crear documento completo ya procesado (todo en una operación)
        public async Task<Documento> CrearDocumentoCompletoAsync(
            byte[] archivoBytes,
            string nombreArchivo,
            int usuarioId,
            int numeroImagenes,
            string resumen,
            string? metadataJson = null,
            string? urlServicioIA = null,
            IWebHostEnvironment? environment = null)
        {
            string filePath = string.Empty;

            // Guardar archivo físicamente si se proporciona environment
            if (environment != null)
            {
                var uploadsFolder = Path.Combine(environment.ContentRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{nombreArchivo}";
                filePath = Path.Combine(uploadsFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, archivoBytes);
            }

            // Crear Documento ya procesado
            var documento = new Documento
            {
                UsuarioId = usuarioId,
                NombreArchivo = nombreArchivo,
                TipoDocumento = Path.GetExtension(nombreArchivo),
                RutaArchivo = filePath,
                FechaSubida = DateTime.UtcNow,
                Procesado = true,
                ArchivoDocumento = archivoBytes,
                TamañoArchivo = archivoBytes.Length,
                EstadoProcesamiento = "completado",
                NumeroImagenes = numeroImagenes,
                ResumenDocumento = resumen,
                ArchivoMetadataJson = metadataJson,
                UrlServicioIA = urlServicioIA,
                FechaProcesamiento = DateTime.UtcNow
            };

            await _documentoRepository.AgregarAsync(documento);
            return documento;
        }

        // Método para obtener estadísticas generales
        public async Task<object> ObtenerEstadisticasGeneralesAsync()
        {
            var documentos = (await _documentoRepository.ObtenerTodosAsync()).ToList();

            return new
            {
                TotalDocumentos = documentos.Count,
                Completados = documentos.Count(d => d.EstadoProcesamiento == "completado"),
                Procesando = documentos.Count(d => d.EstadoProcesamiento == "procesando"),
                ConError = documentos.Count(d => d.EstadoProcesamiento == "error"),
                Pendientes = documentos.Count(d => d.EstadoProcesamiento == "pendiente"),
                TotalUsuarios = documentos.Select(d => d.UsuarioId).Distinct().Count(),
                TamañoTotalBytes = documentos.Sum(d => d.TamañoArchivo ?? 0),
                UltimaActualizacion = DateTime.UtcNow
            };
        }

        // Método para buscar documentos por término
        public async Task<IEnumerable<Documento>> BuscarDocumentosAsync(string terminoBusqueda)
        {
            var todosDocumentos = await _documentoRepository.ObtenerTodosAsync();
            
            return todosDocumentos.Where(d => 
                d.NombreArchivo.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase) ||
                (d.ResumenDocumento != null && d.ResumenDocumento.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase)) ||
                (d.TipoDocumento != null && d.TipoDocumento.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Método para obtener documentos recientes
        public async Task<IEnumerable<Documento>> ObtenerRecientesAsync(int cantidad = 10)
        {
            var documentos = (await _documentoRepository.ObtenerTodosAsync())
                .OrderByDescending(d => d.FechaSubida)
                .Take(cantidad);
                
            return documentos;
        }

        // Método para limpiar documentos antiguos (útil para mantenimiento)
        public async Task<int> EliminarDocumentosAntiguosAsync(DateTime fechaLimite)
        {
            var documentos = await _documentoRepository.ObtenerTodosAsync();
            var documentosAntiguos = documentos.Where(d => d.FechaSubida < fechaLimite).ToList();
            
            int eliminados = 0;
            foreach (var documento in documentosAntiguos)
            {
                // Eliminar archivo físico si existe
                if (!string.IsNullOrEmpty(documento.RutaArchivo) && System.IO.File.Exists(documento.RutaArchivo))
                {
                    try
                    {
                        System.IO.File.Delete(documento.RutaArchivo);
                    }
                    catch
                    {
                        // Continuar aunque falle la eliminación del archivo físico
                    }
                }
                
                await _documentoRepository.EliminarAsync(documento.Id);
                eliminados++;
            }
            
            return eliminados;
        }

        // 🔹 MÉTODO MANTENIDO POR COMPATIBILIDAD (pero ya no se usa en la nueva lógica)
        public async Task<Documento> SubirDocumentoAsync(IFormFile archivo, int usuarioId, IWebHostEnvironment environment)
        {
            return await SubirDocumentoBasicoAsync(archivo, usuarioId, environment);
        }
    }
}