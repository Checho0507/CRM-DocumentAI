using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class InsightsHistoRepository : IInsightsHistoRepository
    {
        private readonly ApplicationDbContext _context;

        public InsightsHistoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InsightsHisto> CreateAsync(InsightsHisto entity)
        {
            _context.InsightsHisto.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<InsightsHisto?> GetByIdAsync(int id)
        {
            return await _context.InsightsHisto
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<InsightsHisto>> GetByUserIdAsync(int userId)
        {
            return await _context.InsightsHisto
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<InsightsHisto>> GetAllAsync()
        {
            return await _context.InsightsHisto
                .OrderByDescending(x => x.Date)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(InsightsHisto entity)
        {
            _context.InsightsHisto.Update(entity);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.InsightsHisto.FindAsync(id);
            if (item == null)
                return false;

            _context.InsightsHisto.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
