using Microsoft.EntityFrameworkCore;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class InsghtRepository : IInsightRepository
    {
        private readonly ApplicationDbContext _context;

        public InsghtRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Insight insight)
        {
            await _context.Insights.AddAsync(insight);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Insight>> ObtenerPorDocumentoIdAsync(int documentoId)
        {
            return await _context.Insights
                .Where(i => i.DocumentoId == documentoId)
                .ToListAsync();
        }

        public async Task<Insight?> ObtenerPorIdAsync(int id)
        {
            return await _context.Insights.FindAsync(id);
        }
    }
}