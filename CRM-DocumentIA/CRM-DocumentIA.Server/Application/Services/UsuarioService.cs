// Application/Services/UsuarioService.cs

using CRM_DocumentIA.Server.Application.DTOs.Usuario;
using CRM_DocumentIA.Server.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class UsuarioService // Se elimina la interfaz IServicioUsuario
    {
        private readonly IUsuarioRepository _repositorioUsuario;

        public UsuarioService(IUsuarioRepository repositorioUsuario)
        {
            _repositorioUsuario = repositorioUsuario;
        }

        public async Task<UsuarioDTO?> ObtenerPerfilAsync(int idUsuario)
        {
            var usuario = await _repositorioUsuario.ObtenerPorIdAsync(idUsuario);

            if (usuario == null) return null;

            // Mapeo manual a DTO
            return new UsuarioDTO
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email.Valor,
                Rol = usuario.Rol,
                DobleFactorActivado = usuario.DobleFactorActivado
            };
        }

        public async Task ActualizarPerfilAsync(int idUsuario, ActualizacionUsuarioDTO dto)
        {
            var usuario = await _repositorioUsuario.ObtenerPorIdAsync(idUsuario);

            if (usuario == null)
                throw new KeyNotFoundException($"Usuario con Id {idUsuario} no encontrado.");

            if (!string.IsNullOrEmpty(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (dto.DobleFactorActivado.HasValue) // 👈 si mandas este campo desde el front
                usuario.DobleFactorActivado = dto.DobleFactorActivado.Value;

            await _repositorioUsuario.ActualizarAsync(usuario);
        }
    }
}