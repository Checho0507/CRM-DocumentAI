// Infrastructure/Repositories/UsuarioRepository.cs

using CRM_DocumentIA.Domain.ValueObjects;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            // Normalizar y crear el ValueObject Email para evitar mismatches de tipo
            var emailNormalizado = email.Trim().ToLowerInvariant();
            var emailVO = new Email(emailNormalizado);

            // Comparación directa usando el ValueObject para que EF utilice el ValueConverter apropiadamente
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == emailVO);


        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }
    }
}