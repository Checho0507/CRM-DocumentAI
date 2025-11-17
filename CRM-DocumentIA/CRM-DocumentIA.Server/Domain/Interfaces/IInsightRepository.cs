using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IInsightRepository
    {
        Task<IEnumerable<Insight>> ObtenerTodosAsync();
        Task<Insight?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Insight>> ObtenerPorDocumentoIdAsync(int documentoId);
        Task<IEnumerable<Insight>> ObtenerPorTipoAsync(string tipoInsight);
        Task<IEnumerable<Insight>> ObtenerPorProcesoIAIdAsync(int procesoIAId);
        Task AgregarAsync(Insight insight);
        Task ActualizarAsync(Insight insight);
        Task EliminarAsync(Insight insight);
    }
}