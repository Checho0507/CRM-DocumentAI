using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IInsightsHistoRepository
    {
        Task<InsightsHisto> CreateAsync(InsightsHisto entity);
        Task<InsightsHisto?> GetByIdAsync(int id);
        Task<IEnumerable<InsightsHisto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<InsightsHisto>> GetAllAsync();
        Task<bool> UpdateAsync(InsightsHisto entity);
        Task<bool> DeleteAsync(int id);
    }
}
