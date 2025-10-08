using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IInsightRepository
    {
        Task AgregarAsync(Insight insight);
        Task<List<Insight>> ObtenerPorDocumentoIdAsync(int documentoId);
        Task<Insight?> ObtenerPorIdAsync(int id);
    }
}