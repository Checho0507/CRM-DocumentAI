// Infrastructure/Repositories/DocumentoCompartidoRepository.cs
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class DocumentoCompartidoRepository : IDocumentoCompartidoRepository
    {
        private readonly ApplicationDbContext _context;

        public DocumentoCompartidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DocumentoCompartido> AgregarAsync(DocumentoCompartido documentoCompartido)
        {
            _context.DocumentosCompartidos.Add(documentoCompartido);
            await _context.SaveChangesAsync();
            return documentoCompartido;
        }

        // Infrastructure/Repositories/DocumentoCompartidoRepository.cs - Agregar este método
        public async Task<IEnumerable<DocumentoCompartido>> ObtenerTodosAsync()
        {
            return await _context.DocumentosCompartidos
                .Include(dc => dc.Documento)
                .Include(dc => dc.UsuarioPropietario)
                .Include(dc => dc.UsuarioCompartido)
                .OrderByDescending(dc => dc.FechaCompartido)
                .ToListAsync();
        }

        public async Task<DocumentoCompartido?> ObtenerPorIdAsync(int id)
        {
            return await _context.DocumentosCompartidos
                .Include(dc => dc.Documento)
                .Include(dc => dc.UsuarioPropietario)
                .Include(dc => dc.UsuarioCompartido)
                .FirstOrDefaultAsync(dc => dc.Id == id);
        }

        public async Task<IEnumerable<DocumentoCompartido>> ObtenerDocumentosCompartidosConUsuarioAsync(int usuarioId)
        {
            return await _context.DocumentosCompartidos
                .Include(dc => dc.Documento)
                .Include(dc => dc.UsuarioPropietario)
                .Include(dc => dc.UsuarioCompartido)
                .Where(dc => dc.UsuarioCompartidoId == usuarioId)
                .OrderByDescending(dc => dc.FechaCompartido)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentoCompartido>> ObtenerDocumentosCompartidosPorPropietarioAsync(int usuarioPropietarioId)
        {
            return await _context.DocumentosCompartidos
                .Include(dc => dc.Documento)
                .Include(dc => dc.UsuarioPropietario)
                .Include(dc => dc.UsuarioCompartido)
                .Where(dc => dc.UsuarioPropietarioId == usuarioPropietarioId)
                .OrderByDescending(dc => dc.FechaCompartido)
                .ToListAsync();
        }

        public async Task<IEnumerable<DocumentoCompartido>> ObtenerCompartidosPorDocumentoAsync(int documentoId)
        {
            return await _context.DocumentosCompartidos
                .Include(dc => dc.Documento)
                .Include(dc => dc.UsuarioPropietario)
                .Include(dc => dc.UsuarioCompartido)
                .Where(dc => dc.DocumentoId == documentoId)
                .OrderByDescending(dc => dc.FechaCompartido)
                .ToListAsync();
        }

        public async Task<bool> ExisteCompartidoAsync(int documentoId, int usuarioCompartidoId)
        {
            return await _context.DocumentosCompartidos
                .AnyAsync(dc => dc.DocumentoId == documentoId && dc.UsuarioCompartidoId == usuarioCompartidoId);
        }

        public async Task EliminarAsync(int id)
        {
            var documentoCompartido = await _context.DocumentosCompartidos.FindAsync(id);
            if (documentoCompartido != null)
            {
                _context.DocumentosCompartidos.Remove(documentoCompartido);
                await _context.SaveChangesAsync();
            }
        }
    }
}