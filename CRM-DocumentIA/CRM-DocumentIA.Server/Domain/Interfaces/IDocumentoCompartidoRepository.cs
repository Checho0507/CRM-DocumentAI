// Domain/Interfaces/IDocumentoCompartidoRepository.cs
using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IDocumentoCompartidoRepository
    {
        Task<DocumentoCompartido> AgregarAsync(DocumentoCompartido documentoCompartido);
        Task<DocumentoCompartido?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<DocumentoCompartido>> ObtenerDocumentosCompartidosConUsuarioAsync(int usuarioId);
        Task<IEnumerable<DocumentoCompartido>> ObtenerDocumentosCompartidosPorPropietarioAsync(int usuarioPropietarioId);
        Task<IEnumerable<DocumentoCompartido>> ObtenerCompartidosPorDocumentoAsync(int documentoId);
        Task<bool> ExisteCompartidoAsync(int documentoId, int usuarioCompartidoId);
        Task EliminarAsync(int id);
        Task<IEnumerable<DocumentoCompartido>> ObtenerTodosAsync();
    }
}