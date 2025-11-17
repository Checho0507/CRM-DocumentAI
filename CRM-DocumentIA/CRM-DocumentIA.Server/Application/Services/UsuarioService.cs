using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            return await _usuarioRepository.ObtenerPorEmailAsync(email);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(int id)
        {
            return await _usuarioRepository.ObtenerPorIdAsync(id);
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        {
            return await _usuarioRepository.ObtenerTodosAsync();
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            await _usuarioRepository.AgregarAsync(usuario);
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            await _usuarioRepository.ActualizarAsync(usuario);
        }

        public async Task EliminarAsync(int id)
        {
            await _usuarioRepository.EliminarAsync(id);
        }

        // ✅ AGREGAR ESTOS MÉTODOS FALTANTES
        public async Task<Usuario?> ObtenerPerfilAsync(int usuarioId)
        {
            return await _usuarioRepository.ObtenerPorIdAsync(usuarioId);
        }

        public async Task ActualizarPerfilAsync(Usuario usuario)
        {
            await _usuarioRepository.ActualizarAsync(usuario);
        }


        public async Task<bool> AsignarRolAsync(int usuarioId, int rolId)
        {
            // Opcional: puedes verificar si el rol existe usando IRolRepository
            return await _usuarioRepository.AsignarRolAsync(usuarioId, rolId);
        }
    }
}