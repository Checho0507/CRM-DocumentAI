using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

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
    }
}