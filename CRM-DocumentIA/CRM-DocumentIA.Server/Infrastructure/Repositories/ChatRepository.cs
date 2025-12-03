using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces.Repositories;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Chat> CreateAsync(Chat chat)
        {
            await _context.Chats.AddAsync(chat);
            return chat;
        }

        public async Task<Chat?> GetByIdAsync(int id)
        {
            return await _context.Chats
                .Include(c => c.Insights)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Chat>> GetByUserAsync(int userId)
        {
            return await _context.Chats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var chat = await _context.Chats.FindAsync(id);
            if (chat == null)
                return false;

            _context.Chats.Remove(chat);
            return true; // confirm deletion, real save happens in SaveChangesAsync
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
