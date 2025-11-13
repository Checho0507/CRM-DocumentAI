using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IProcesoIARepository
    {
        Task<IEnumerable<ProcesoIA>> ObtenerTodosAsync();
        Task<ProcesoIA?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<ProcesoIA>> ObtenerPorDocumentoIdAsync(int documentoId);
        Task<IEnumerable<ProcesoIA>> ObtenerPorEstadoAsync(string estado);
        Task AgregarAsync(ProcesoIA procesoIA);
        Task ActualizarAsync(ProcesoIA procesoIA);
        Task EliminarAsync(int id);
    }
}