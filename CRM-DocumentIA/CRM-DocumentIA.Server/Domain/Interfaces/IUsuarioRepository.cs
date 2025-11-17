using System.Threading.Tasks;
using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task AgregarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task EliminarAsync(int id);
        Task<bool> AsignarRolAsync(int usuarioId, int rolId);
    }
}