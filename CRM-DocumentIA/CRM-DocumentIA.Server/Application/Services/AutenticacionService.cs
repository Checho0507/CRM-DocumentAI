// Application/Services/AuthenticationService.cs

using BCrypt.Net;
using CRM_DocumentIA.Server.Application.DTOs.Auth;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;
using CRM_DocumentIA.Domain.ValueObjects;
using CRM_DocumentIA.Server.Application.Services;

namespace CRM_DocumentIA.Application.Services
{
    // Clase principal renombrada a AuthenticationService
    public class AutenticacionService
    {
        // Renombrando campos privados para seguir convenciones (camelCase con prefijo '_')
        private readonly IUsuarioRepository _userRepository;
        private readonly JWTService _jwtService; // Asumiendo que has renombrado ServicioJwt a JwtService

        public AutenticacionService(IUsuarioRepository userRepository, JWTService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task RegistrarUsuarioAsync(RegistroDTO dto)
        {
            // 1. Verificar si el usuario ya existe
            var usuarioExistente = await _userRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuarioExistente != null)
            {
                // En una app real, podrías lanzar una excepción de negocio
                throw new ArgumentException("El email ya está registrado.");
            }

            // 2. Crear hash seguro de la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Crear la entidad Usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = new Email(dto.Email), // Usa el Value Object
                PasswordHash = passwordHash,
                Rol = "usuario"
            };

            // 4. Guardar en la base de datos
            await _userRepository.AgregarAsync(nuevoUsuario);
        }

        public async Task<RespuestaAuthDTO> LoginAsync(LoginDTO dto)
        {
            // 1. Buscar el usuario
            var usuario = await _userRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 2. Verificar la contraseña
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 3. Generar el JWT
            var token = _jwtService.GenerarToken(usuario);

            // 4. Devolver la respuesta esperada por NextAuth
            return new RespuestaAuthDTO
            {
                Token = token,
                Usuario = new UsuarioInfoDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email.Valor,
                    Nombre = usuario.Nombre,
                    Rol = usuario.Rol
                }
            };
        }

        // 5. Método para NextAuth (Google Login, etc.)
        public async Task<UsuarioInfoDTO?> ValidarUsuarioExternoAsync(string email)
        {
            var usuario = await _userRepository.ObtenerPorEmailAsync(email);

            if (usuario == null) return null;

            // Aquí podrías generar un JWT si fuera necesario para un flujo "check-user"
            return new UsuarioInfoDTO
            {
                Id = usuario.Id,
                Email = usuario.Email.Valor,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol
            };
        }
    }
}