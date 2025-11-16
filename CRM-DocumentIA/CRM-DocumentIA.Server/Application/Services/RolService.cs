using CRM_DocumentIA.Server.Application.DTOs.Rol;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class RolService
    {
        private readonly IRolRepository _rolRepository;

        public RolService(IRolRepository rolRepository)
        {
            _rolRepository = rolRepository;
        }

        public async Task<List<RolDTO>> ObtenerTodosAsync()
        {
            var roles = await _rolRepository.ObtenerTodosAsync();

            return roles.Select(r => new RolDTO
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion
            }).ToList();
        }

        public async Task<RolDTO?> ObtenerPorIdAsync(int id)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(id);
            if (rol == null) return null;

            return new RolDTO
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion
            };
        }

        public async Task<RolDTO> CrearAsync(CrearRolDTO dto)
        {
            var rol = new Rol(dto.Nombre, dto.Descripcion);
            rol = await _rolRepository.CrearAsync(rol);

            return new RolDTO
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion
            };
        }

        public async Task<RolDTO?> ActualizarAsync(int id, ActualizarRolDTO dto)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(id);
            if (rol == null) return null;

            rol.Nombre = dto.Nombre;
            rol.Descripcion = dto.Descripcion;

            rol = await _rolRepository.ActualizarAsync(rol);

            return new RolDTO
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion
            };
        }

        public async Task<bool> EliminarAsync(int id)
        {
            return await _rolRepository.EliminarAsync(id);
        }
    }
}
