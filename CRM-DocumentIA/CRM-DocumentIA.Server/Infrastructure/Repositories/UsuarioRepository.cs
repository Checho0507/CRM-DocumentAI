using CRM_DocumentIA.Server.Application.DTOs.Analytics;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.CodeDom;
using System.Threading.Tasks;

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
            => await _context.Usuarios
                .Include(u => u.Rol)  // 🔥 Agregué Include para Rol
                .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Usuario?> ObtenerPorEmailConRolAsync(string email)
        {
            return await _context.Usuarios
                .Include(u => u.Rol) // ✔ único include necesario
                .FirstOrDefaultAsync(u => u.Email.Value == email);
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            try
            {
                return await _context.Usuarios
                    .Include(u => u.Rol)  // 🔥 Agregué Include para consistencia
                    .FirstOrDefaultAsync(u => u.Email.Value == email);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
            => await _context.Usuarios
                .Include(u => u.Rol)  // 🔥 Agregué Include para Rol
                .ToListAsync();

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

        // 🔥 AGREGAR ESTE MÉTODO FALTANTE
        public async Task<IEnumerable<Usuario>> BuscarPorNombreOEmailAsync(string busqueda)
        {
            if (string.IsNullOrWhiteSpace(busqueda))
                return new List<Usuario>();

            // Convertir a minúsculas para búsqueda case-insensitive
            var busquedaLower = busqueda.ToLower();

            return await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u =>
                    (u.Nombre != null && u.Nombre.ToLower().Contains(busquedaLower)) ||
                    (u.Email != null && u.Email.Value != null && u.Email.Value.ToLower().Contains(busquedaLower))
                )
                .OrderBy(u => u.Nombre)
                .Take(20)
                .ToListAsync();
        }

        // Analítica

        public async Task<DateTime?> GetLastActivityDateAsync(int userId)
        {
            return await _context.Chats
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => (DateTime?)c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Usuarios.CountAsync();
        }
    }
}