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
    }
}
