using CRM_DocumentIA.Server.Domain.Entities;
namespace CRM_DocumentIA.Server.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task<Usuario?> ObtenerPorEmailAsync(string email);
    }
}
