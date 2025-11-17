using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
            => await _context.Usuarios.FindAsync(id);

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
            => await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.Value == email);

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
            => await _context.Usuarios.ToListAsync();

        public async Task AgregarAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AsignarRolAsync(int usuarioId, int rolId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                return false;

            usuario.RolId = rolId;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}