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

        public async Task<int> CountAsync()
        {
            return await _context.InsightsHisto.CountAsync();
        }

        public async Task<int> CountDistinctUsersLastDaysAsync(int days)
        {
            var dateLimit = DateTime.UtcNow.AddDays(-days);

            return await _context.InsightsHisto
                .Where(x => x.Date >= dateLimit)
                .Select(x => x.UserId)
                .Distinct()
                .CountAsync();
        }

        public async Task<IEnumerable<ConsultasPorDiaResult>> GetConsultasGroupedByDayAsync()
        {
            return await _context.InsightsHisto
                .GroupBy(x => x.Date.Date)
                .Select(g => new ConsultasPorDiaResult
                {
                    Fecha = g.Key,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopUsuariosResult>> GetTopUsersAsync(int top)
        {
            return await _context.InsightsHisto
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .Join(
                    _context.Usuarios,
                    a => a.UserId,
                    u => u.Id,
                    (a, u) => new TopUsuariosResult
                    {
                        UserId = u.Id,
                        Nombre = u.Nombre,
                        Total = a.Total
                    }
                )
                .ToListAsync();
        }
    }
}
