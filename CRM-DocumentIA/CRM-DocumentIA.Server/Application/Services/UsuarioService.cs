// Application/Services/UsuarioService.cs
using CRM_DocumentIA.Server.Application.DTOs.Usuario;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRM_DocumentIA.Server.Application.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Usuario>> ObtenerTodosAsync();
        Task AgregarAsync(Usuario usuario);
        Task ActualizarAsync(Usuario usuario);
        Task EliminarAsync(int id);
        Task<Usuario?> ObtenerPerfilAsync(int usuarioId);
        Task ActualizarPerfilAsync(Usuario usuario);
        Task<bool> AsignarRolAsync(int usuarioId, int rolId);
        Task<IEnumerable<UsuarioDTO>> BuscarUsuariosAsync(string busqueda, int usuarioActualId);
        Task<IEnumerable<UsuarioDTO>> BuscarUsuariosConRolAsync(string busqueda, int usuarioActualId);
        Task ContarUsuariosPorRolAsync();
    }

    public class UsuarioService : IUsuarioService
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
            return await _usuarioRepository.AsignarRolAsync(usuarioId, rolId);
        }

        public async Task<IEnumerable<UsuarioDTO>> BuscarUsuariosAsync(string busqueda, int usuarioActualId)
        {
            var usuarios = await _usuarioRepository.BuscarPorNombreOEmailAsync(busqueda);
            return usuarios
                .Where(u => u.Id != usuarioActualId) // Excluir al usuario actual
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email?.Value ?? string.Empty,
                    RolId = u.RolId,
                    RolNombre = u.Rol?.Nombre ?? "Sin rol",
                    DobleFactorActivado = u.DobleFactorActivado
                });
        }

        public async Task<IEnumerable<UsuarioDTO>> BuscarUsuariosConRolAsync(string busqueda, int usuarioActualId)
        {
            var usuarios = await _usuarioRepository.BuscarPorNombreOEmailAsync(busqueda);
            return usuarios
                .Where(u => u.Id != usuarioActualId)
                .Select(u => new UsuarioDTO
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email?.Value ?? string.Empty,
                    RolId = u.RolId,
                    RolNombre = u.Rol?.Nombre ?? "Sin rol",
                    DobleFactorActivado = u.DobleFactorActivado
                })
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Email);
        }

        // Método para contar usuarios por rol (opcional, para estadísticas)
        public async Task<Dictionary<string, int>> ContarUsuariosPorRolAsync()
        {
            var usuarios = await _usuarioRepository.ObtenerTodosAsync();
            return usuarios
                .GroupBy(u => u.Rol?.Nombre ?? "Sin rol")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // Método para obtener usuarios con última actividad (opcional)
        public async Task<IEnumerable<object>> ObtenerUsuariosConUltimaActividadAsync()
        {
            var usuarios = await _usuarioRepository.ObtenerTodosAsync();
            var resultado = new List<object>();
            foreach (var usuario in usuarios)
            {
                var ultimaActividad = await _usuarioRepository.GetLastActivityDateAsync(usuario.Id);
                resultado.Add(new
                {
                    usuario.Id,
                    usuario.Nombre,
                    Email = usuario.Email?.Value ?? string.Empty,
                    Rol = usuario.Rol?.Nombre ?? "Sin rol",
                    UltimaActividad = ultimaActividad?.ToString("yyyy-MM-dd HH:mm") ?? "Nunca",
                    Activo = usuario,
                    DobleFactorActivado = usuario.DobleFactorActivado
                });
            }
            return resultado;
        }

        Task IUsuarioService.ContarUsuariosPorRolAsync()
        {
            return ContarUsuariosPorRolAsync();
        }
    }
}