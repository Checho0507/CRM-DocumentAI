using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces.Repositories
{
    public interface IChatRepository
    {
        Task<Chat> CreateAsync(Chat chat);
        Task<Chat?> GetByIdAsync(int id);
        Task<IEnumerable<Chat>> GetByUserAsync(int userId);
        Task<bool> DeleteAsync(int id);
        Task SaveChangesAsync();

        Task<int> CountAsync();
    }
}
