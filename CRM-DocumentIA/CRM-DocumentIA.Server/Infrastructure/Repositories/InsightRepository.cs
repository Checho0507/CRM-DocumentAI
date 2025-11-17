using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class InsightRepository : IInsightRepository
    {
        private readonly ApplicationDbContext _context;

        public InsightRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Insight>> ObtenerTodosAsync()
            => await _context.Insights
                .Include(i => i.Documento)
                .Include(i => i.ProcesoIA)
                .OrderByDescending(i => i.FechaGeneracion)
                .ToListAsync();

        public async Task<Insight?> ObtenerPorIdAsync(int id)
            => await _context.Insights
                .Include(i => i.Documento)
                .Include(i => i.ProcesoIA)
                .FirstOrDefaultAsync(i => i.Id == id);

        public async Task<IEnumerable<Insight>> ObtenerPorDocumentoIdAsync(int documentoId)
            => await _context.Insights
                .Where(i => i.DocumentoId == documentoId)
                .Include(i => i.Documento)
                .Include(i => i.ProcesoIA)
                .OrderByDescending(i => i.FechaGeneracion)
                .ToListAsync();

        public async Task<IEnumerable<Insight>> ObtenerPorTipoAsync(string tipoInsight)
            => await _context.Insights
                .Where(i => i.TipoInsight == tipoInsight)
                .Include(i => i.Documento)
                .Include(i => i.ProcesoIA)
                .OrderByDescending(i => i.FechaGeneracion)
                .ToListAsync();

        public async Task<IEnumerable<Insight>> ObtenerPorProcesoIAIdAsync(int procesoIAId)
            => await _context.Insights
                .Where(i => i.ProcesoIAId == procesoIAId)
                .Include(i => i.Documento)
                .Include(i => i.ProcesoIA)
                .OrderByDescending(i => i.FechaGeneracion)
                .ToListAsync();

        public async Task AgregarAsync(Insight insight)
        {
            await _context.Insights.AddAsync(insight);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Insight insight)
        {
            _context.Insights.Update(insight);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Insight insight)
        {
            _context.Insights.Remove(insight);
            await _context.SaveChangesAsync();
        }
    }
}