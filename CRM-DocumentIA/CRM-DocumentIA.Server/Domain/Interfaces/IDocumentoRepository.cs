using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IDocumentoRepository
    {
        Task<IEnumerable<Documento>> ObtenerTodosAsync();
        Task<Documento?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Documento documento);
        Task ActualizarAsync(Documento documento);
        Task EliminarAsync(int id);
    }
}
