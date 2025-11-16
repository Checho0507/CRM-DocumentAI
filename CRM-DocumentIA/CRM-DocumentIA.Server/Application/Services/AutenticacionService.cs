using System;
using System.Threading.Tasks;
using BCrypt.Net;
using CRM_DocumentIA.Domain.ValueObjects;
using CRM_DocumentIA.Server.Application.DTOs.Auth;
using CRM_DocumentIA.Server.Application.DTOs._2FA;
using CRM_DocumentIA.Server.Application.Services;
using CRM_DocumentIA.Server.Domain.Entities;
using CRM_DocumentIA.Server.Domain.Interfaces;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class AutenticacionService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly JWTService _jwtService;
        private readonly SmtpEmailService _smtpEmailService;

        public AutenticacionService(
            IUsuarioRepository usuarioRepository,
            JWTService jwtService,
            SmtpEmailService smtpEmailService)
        {
            _usuarioRepository = usuarioRepository;
            _jwtService = jwtService;
            _smtpEmailService = smtpEmailService;
        }

        // ===============================
        // 🔐 REGISTRO DE USUARIO
        // ===============================
        public async Task RegistrarUsuarioAsync(RegistroDTO dto)
        {
            var usuarioExistente = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuarioExistente != null)
                throw new ArgumentException("El email ya está registrado.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var nuevoUsuario = new Usuario(
                new Email(dto.Email),
                dto.Nombre,
                passwordHash,
                1
            )
            {
                DobleFactorActivado = dto.DobleFactorActivado // Se guarda el estado del 2FA
            };

            await _usuarioRepository.AgregarAsync(nuevoUsuario);
        }

        // ===============================
        // 🔑 LOGIN TRADICIONAL
        // ===============================
        public async Task<RespuestaAuthDTO> LoginAsync(LoginDTO dto)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            var token = _jwtService.GenerarToken(usuario);

            return new RespuestaAuthDTO
            {
                Token = token,
                Usuario = new UsuarioInfoDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email.Value,
                    Nombre = usuario.Nombre,
                    RolId = usuario.Rol.Id,
                    RolNombre = usuario.Rol.Nombre,
                },
                DobleFactorActivado = usuario.DobleFactorActivado
            };
        }

        // ===============================
        // 🌐 LOGIN SOCIAL (GOOGLE, ETC.)
        // ===============================
        public async Task<RespuestaAuthDTO> LoginSocialAsync(LoginSocialDTO dto)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(dto.Email);

            if (usuario == null)
            {
                var emailVo = new Email(dto.Email);
                var passwordDummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

                var nuevoUsuario = new Usuario(
                    emailVo,
                    dto.Name,
                    passwordDummyHash,
                    1
                )
                {
                    DobleFactorActivado = false // Por defecto desactivado
                };

                await _usuarioRepository.AgregarAsync(nuevoUsuario);
                usuario = nuevoUsuario;
            }

            var token = _jwtService.GenerarToken(usuario);

            return new RespuestaAuthDTO
            {
                Token = token,
                Usuario = new UsuarioInfoDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email.Value,
                    Nombre = usuario.Nombre,
                    RolId = usuario.Rol.Id,
                    RolNombre = usuario.Rol.Nombre,
                },
                DobleFactorActivado = usuario.DobleFactorActivado
            };
        }

        // ===============================
        // 📩 ENVÍO DE CÓDIGO 2FA
        // ===============================
        public async Task EnviarCodigo2FAAsync(string email, string codigo)
        {
            var subject = "Código de verificación - CRM DocumentIA";
            var body = $@"
                <p>Tu código de verificación es: <strong>{codigo}</strong></p>
                <p>Expira en 5 minutos.</p>
                <p>Si no solicitaste este código, ignora este mensaje.</p>";

            await _smtpEmailService.SendEmailAsync(email, subject, body);
        }
    }
}
