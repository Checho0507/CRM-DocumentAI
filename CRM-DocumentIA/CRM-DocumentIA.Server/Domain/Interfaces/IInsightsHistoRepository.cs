using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public class ConsultasPorDiaResult
    {
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
    }

    public class TopUsuariosResult
    {
        public int UserId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Total { get; set; }
    }


    public interface IInsightsHistoRepository
    {
        Task<InsightsHisto> CreateAsync(InsightsHisto entity);
        Task<InsightsHisto?> GetByIdAsync(int id);
        Task<IEnumerable<InsightsHisto>> GetByUserIdAsync(int userId);
        Task<IEnumerable<InsightsHisto>> GetAllAsync();
        Task<bool> UpdateAsync(InsightsHisto entity);
        Task<bool> DeleteAsync(int id);
        Task<int> CountAsync();
        Task<int> CountDistinctUsersLastDaysAsync(int days);
        Task<IEnumerable<ConsultasPorDiaResult>> GetConsultasGroupedByDayAsync();
        Task<IEnumerable<TopUsuariosResult>> GetTopUsersAsync(int top);
    }
}
