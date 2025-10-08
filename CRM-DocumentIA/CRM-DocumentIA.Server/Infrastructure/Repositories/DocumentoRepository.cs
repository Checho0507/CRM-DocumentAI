using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class ProcesoIARepository : IProcesoIARepository
    {
        private readonly ApplicationDbContext _context;

        public ProcesoIARepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProcesoIA>> ObtenerTodosAsync()
            => await _context.ProcesosIA.Include(p => p.Documento).ToListAsync();

        public async Task<ProcesoIA?> ObtenerPorIdAsync(int id)
            => await _context.ProcesosIA.Include(p => p.Documento)
                                        .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AgregarAsync(ProcesoIA procesoIA)
        {
            await _context.ProcesosIA.AddAsync(procesoIA);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(ProcesoIA procesoIA)
        {
            _context.ProcesosIA.Update(procesoIA);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var proceso = await _context.ProcesosIA.FindAsync(id);
            if (proceso != null)
            {
                _context.ProcesosIA.Remove(proceso);
                await _context.SaveChangesAsync();
            }
        }
    }
}
