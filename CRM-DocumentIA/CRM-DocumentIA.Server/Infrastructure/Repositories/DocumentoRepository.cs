using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class DocumentoRepository : IDocumentoRepository // ✅ Nombre correcto
    {
        private readonly ApplicationDbContext _context;

        public DocumentoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Documento>> ObtenerPorUsuarioIdAsync(int usuarioId)
    => await _context.Documentos
        .Where(d => d.UsuarioId == usuarioId)
        .Include(d => d.Usuario)
        .ToListAsync();
        
        public async Task<IEnumerable<Documento>> ObtenerTodosAsync()
            => await _context.Documentos
                .Include(d => d.Usuario) // ✅ Incluir Usuario
                .ToListAsync();

        public async Task<Documento?> ObtenerPorIdAsync(int id)
            => await _context.Documentos
                .Include(d => d.Usuario) // ✅ Incluir Usuario
                .FirstOrDefaultAsync(d => d.Id == id);

        public async Task AgregarAsync(Documento documento)
        {
            await _context.Documentos.AddAsync(documento);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Documento documento)
        {
            _context.Documentos.Update(documento);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var documento = await _context.Documentos.FindAsync(id);
            if (documento != null)
            {
                _context.Documentos.Remove(documento);
                await _context.SaveChangesAsync();
            }
        }
    }
}