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

        public Task<IEnumerable<Documento>> ObtenerTodosAsync() => _documentoRepository.ObtenerTodosAsync();
        public Task<Documento?> ObtenerPorIdAsync(int id) => _documentoRepository.ObtenerPorIdAsync(id);
        public Task AgregarAsync(Documento documento) => _documentoRepository.AgregarAsync(documento);
        public Task ActualizarAsync(Documento documento) => _documentoRepository.ActualizarAsync(documento);
        public Task EliminarAsync(int id) => _documentoRepository.EliminarAsync(id);

        // 🔹 Nuevo método para manejar carga desde archivos (opcional)
        public async Task<Documento> SubirDocumentoAsync(IFormFile archivo, IFormFile? metadataJson, int clienteId)
        {
            if (archivo == null || archivo.Length == 0)
                throw new ArgumentException("Debe enviar un archivo válido.");

            byte[] archivoBytes;
            using (var ms = new MemoryStream())
            {
                await archivo.CopyToAsync(ms);
                archivoBytes = ms.ToArray();
            }

            string? jsonContent = null;
            if (metadataJson != null)
            {
                using var reader = new StreamReader(metadataJson.OpenReadStream());
                jsonContent = await reader.ReadToEndAsync();
            }

            var documento = new Documento
            {
                ClienteId = clienteId,
                NombreArchivo = archivo.FileName,
                TipoDocumento = Path.GetExtension(archivo.FileName),
                FechaSubida = DateTime.Now,
                ArchivoDocumento = archivoBytes,
                ArchivoMetadataJson = jsonContent
            };

            await _documentoRepository.AgregarAsync(documento);
            return documento;
        }
    }
}
