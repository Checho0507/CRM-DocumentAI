using System; // Necesario para Exception y Guid
using BCrypt.Net;
using CRM_DocumentIA.Domain.ValueObjects;
using CRM_DocumentIA.Server.Application.DTOs.Auth;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Entities; // 🎯 CRÍTICO: Necesario para referenciar 'Usuario'
using CRM_DocumentIA.Server.Domain.Interfaces;

// Nota: Asumo que JWTService implementa IServicioJwt, aunque no se muestre.
// Usaremos AutenticacionService y JWTService según tu código.

namespace CRM_DocumentIA.Application.Services
{
    // Asegúrate de que tu interfaz (si existe) se llama IServicioAutenticacion
    public class AutenticacionService // Asumo que tu clase se llama así, sin 'ServicioAutenticacion'
    {
        // 🎯 UNIFICACIÓN: Usamos un solo repositorio y un solo servicio JWT.
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly JWTService _jwtService;

        // 🎯 UNIFICACIÓN: Usar los nombres de variables del constructor
        public AutenticacionService(IUsuarioRepository userRepository, JWTService jwtService)
        {
            _usuarioRepository = userRepository; // Asumo que inyectas el mismo repo dos veces, corrijo a uno
            _jwtService = jwtService;
        }

        public async Task RegistrarUsuarioAsync(RegistroDTO dto)
        {
            // 1. Verificar si el usuario ya existe
            var usuarioExistente = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuarioExistente != null)
            {
                throw new ArgumentException("El email ya está registrado.");
            }

            // 2. Crear hash seguro de la contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Crear la entidad Usuario usando el CONSTRUCTOR POSICIONAL
            // (Asumo el orden: Email, Nombre, PasswordHash, Rol)
            var nuevoUsuario = new Usuario(
                new Email(dto.Email),
                dto.Nombre,
                passwordHash,
                "usuario"
            );

            // 4. Guardar en la base de datos
            await _usuarioRepository.AgregarAsync(nuevoUsuario);
        }

        public async Task<RespuestaAuthDTO> LoginAsync(LoginDTO dto)
        {
            // 1. Buscar el usuario
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            {
                throw new UnauthorizedAccessException("Credenciales inválidas.");
            }

            // 2. Generar el JWT
            var token = _jwtService.GenerarToken(usuario);

            // 3. Devolver la respuesta
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

        // 🎯 MÉTODO FINAL DE LOGIN SOCIAL (Login O Registro)
        public async Task<RespuestaAuthDTO> LoginSocialAsync(LoginSocialDTO dto)
        {
            // 1. Buscar usuario por email
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);

            if (usuario == null)
            {
                // 2. Si no existe, REGISTRARLO automáticamente
                var emailVo = new Email(dto.Email);
                var passwordDummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

                // 🎯 CRÍTICO: Usamos constructor posicional para evitar CS1739
                var nuevoUsuario = new Usuario(
                    emailVo,
                    dto.Name,
                    passwordDummyHash,
                    "usuario"
                );

                await _usuarioRepository.AgregarAsync(nuevoUsuario);
                usuario = nuevoUsuario;
            }

            // 3. Generar el JWT
            var token = _jwtService.GenerarToken(usuario);

            // 4. Devolver la respuesta
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
    }
}
