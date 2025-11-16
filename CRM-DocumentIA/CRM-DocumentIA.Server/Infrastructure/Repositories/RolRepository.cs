using CRM_DocumentIA.Infrastructure.Database;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Server.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM_DocumentIA.Server.Infrastructure.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly ApplicationDbContext _context;

        public RolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rol>> ObtenerTodosAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Rol> CrearAsync(Rol rol)
        {
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();
            return rol;
        }

        public async Task<Rol> ActualizarAsync(Rol rol)
        {
            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();
            return rol;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return false;

            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
