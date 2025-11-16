using CRM_DocumentIA.Server.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IRolRepository
    {
        Task<List<Rol>> ObtenerTodosAsync();
        Task<Rol?> ObtenerPorIdAsync(int id);
        Task<Rol> CrearAsync(Rol rol);
        Task<Rol> ActualizarAsync(Rol rol);
        Task<bool> EliminarAsync(int id);
    }

}