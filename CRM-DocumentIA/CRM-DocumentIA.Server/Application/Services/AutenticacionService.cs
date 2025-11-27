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
        private readonly TwoFactorService _twoFactorService;

        public AutenticacionService(
            IUsuarioRepository usuarioRepository,
            JWTService jwtService,
            SmtpEmailService smtpEmailService,
            TwoFactorService twoFactorService)
        {
            _usuarioRepository = usuarioRepository;
            _jwtService = jwtService;
            _smtpEmailService = smtpEmailService;
            _twoFactorService = twoFactorService;

            Console.WriteLine("🔵 [AUTH-SERVICE] AutenticacionService inicializado");
            Console.WriteLine($"🔵 [AUTH-SERVICE] _jwtService es null: {_jwtService == null}");
            Console.WriteLine($"🔵 [AUTH-SERVICE] _usuarioRepository es null: {_usuarioRepository == null}");
        }

        // ===============================
        // 🔐 REGISTRO DE USUARIO - CORREGIDO
        // ===============================
        public async Task<RegistroResultDTO> RegistrarUsuarioAsync(RegistroDTO dto)
        {
            Console.WriteLine("🔵 [AUTH-SERVICE] RegistrarUsuarioAsync llamado");

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
                DobleFactorActivado = dto.DobleFactorActivado
            };

            await _usuarioRepository.AgregarAsync(nuevoUsuario);

            // 🔥 GENERAR Y ENVIAR CÓDIGO 2FA SI ESTÁ ACTIVADO
            Guid? twoFaTempId = null;
            if (dto.DobleFactorActivado)
            {
                twoFaTempId = await _twoFactorService.GenerateAndSendAsync(nuevoUsuario.Id, dto.Email);
            }

            return new RegistroResultDTO
            {
                Success = true,
                Message = "Registro exitoso",
                Requires2FA = dto.DobleFactorActivado,
                TwoFaTempId = twoFaTempId
            };
        }

        // ===============================
        // 🔑 LOGIN TRADICIONAL - CORREGIDO CON 2FA
        // ===============================
        public async Task<RespuestaAuthDTO> LoginAsync(LoginDTO dto)
        {
            Console.WriteLine("🔵 [AUTH-SERVICE-LOGIN] INICIO - LoginAsync llamado");
            Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Buscando usuario: {dto?.Email}");

            try
            {
                // Obtener usuario CON ROL incluido
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Llamando a _usuarioRepository.ObtenerPorEmailConRolAsync...");
                var usuario = await _usuarioRepository.ObtenerPorEmailConRolAsync(dto.Email);

                if (usuario == null)
                {
                    Console.WriteLine($"🔴 [AUTH-SERVICE-LOGIN] Usuario no encontrado: {dto.Email}");
                    throw new UnauthorizedAccessException("Credenciales inválidas.");
                }

                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Usuario encontrado: {usuario.Nombre}");
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Rol del usuario: {(usuario.Rol != null ? usuario.Rol.Nombre : "NULL")}");
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Email del usuario: {(usuario.Email != null ? usuario.Email.Value : "NULL")}");
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] PasswordHash: {(usuario.PasswordHash != null ? "PRESENTE" : "NULL")}");
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] 2FA activado: {usuario.DobleFactorActivado}");

                // Verificar contraseña
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Verificando contraseña...");
                bool passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

                if (!passwordValido)
                {
                    Console.WriteLine($"🔴 [AUTH-SERVICE-LOGIN] Contraseña incorrecta para: {dto.Email}");
                    throw new UnauthorizedAccessException("Credenciales inválidas.");
                }

                Console.WriteLine($"🟢 [AUTH-SERVICE-LOGIN] Credenciales válidas para: {dto.Email}");

                // 🔥 NUEVO: Verificar si tiene 2FA activado (igual que en LoginSocial)
                if (usuario.DobleFactorActivado)
                {
                    Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Usuario tiene 2FA activado, enviando código...");

                    // Generar código 2FA
                    var rng = new Random();
                    var codigo2FA = rng.Next(0, 1000000).ToString("D6");

                    // Enviar código por email
                    await EnviarCodigo2FAAsync(usuario.Email.Value, codigo2FA);

                    Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Código 2FA enviado: {codigo2FA}");

                    // 🔥 CORREGIDO: Usar propiedades existentes de RespuestaAuthDTO
                    return new RespuestaAuthDTO
                    {
                        Token = "", // Token vacío porque requiere 2FA
                        Usuario = new UsuarioInfoDTO
                        {
                            Id = usuario.Id,
                            Email = usuario.Email?.Value ?? string.Empty,
                            Nombre = usuario.Nombre ?? string.Empty,
                            RolId = usuario.Rol?.Id ?? 1,
                            RolNombre = usuario.Rol?.Nombre ?? "Usuario"
                        },
                        DobleFactorActivado = true, // Indicar que requiere 2FA
                        // Si necesitas un mensaje, puedes usar una propiedad existente o agregar una nueva
                    };
                }

                // 🔥 CONTINUACIÓN: Si no tiene 2FA, generar token directamente
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Generando token JWT...");
                var token = _jwtService.GenerarToken(usuario);
                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] Token generado: {(string.IsNullOrEmpty(token) ? "VACÍO" : "PRESENTE")}");

                var usuarioInfo = new UsuarioInfoDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email?.Value ?? string.Empty,
                    Nombre = usuario.Nombre ?? string.Empty,
                    RolId = usuario.Rol?.Id ?? 1,
                    RolNombre = usuario.Rol?.Nombre ?? "Usuario"
                };

                Console.WriteLine($"🔵 [AUTH-SERVICE-LOGIN] UsuarioInfoDTO creado - RolId: {usuarioInfo.RolId}, RolNombre: {usuarioInfo.RolNombre}");

                var respuesta = new RespuestaAuthDTO
                {
                    Token = token,
                    Usuario = usuarioInfo,
                    DobleFactorActivado = false
                };

                Console.WriteLine($"🟢 [AUTH-SERVICE-LOGIN] LoginAsync completado exitosamente");
                return respuesta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 [AUTH-SERVICE-LOGIN] Error en LoginAsync: {ex.Message}");
                Console.WriteLine($"🔴 [AUTH-SERVICE-LOGIN] Tipo de excepción: {ex.GetType().Name}");
                Console.WriteLine($"🔴 [AUTH-SERVICE-LOGIN] StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"🔴 [AUTH-SERVICE-LOGIN] Inner Exception: {ex.InnerException.Message}");
                }

                throw;
            }
        }

        // ===============================
        // 🌐 LOGIN SOCIAL (GOOGLE, ETC.) - CORREGIDO CON 2FA
        // ===============================
        public async Task<RespuestaAuthDTO> LoginSocialAsync(LoginSocialDTO dto)
        {
            Console.WriteLine("🔵 [AUTH-SERVICE-SOCIAL] INICIO - LoginSocialAsync llamado");
            Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Email: {dto?.Email}, Nombre: {dto?.Name}");

            try
            {
                // Obtener usuario CON ROL incluido
                var usuario = await _usuarioRepository.ObtenerPorEmailConRolAsync(dto.Email);

                if (usuario == null)
                {
                    Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Usuario no existe, creando nuevo...");
                    var emailVo = new Email(dto.Email);
                    var passwordDummyHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

                    var nuevoUsuario = new Usuario(
                        emailVo,
                        dto.Name,
                        passwordDummyHash,
                        1 // Rol por defecto
                    )
                    {
                        DobleFactorActivado = false // Por defecto, nuevos usuarios no tienen 2FA
                    };

                    await _usuarioRepository.AgregarAsync(nuevoUsuario);
                    usuario = nuevoUsuario;

                    Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Nuevo usuario creado: {usuario.Nombre}");
                }
                else
                {
                    Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Usuario existente encontrado: {usuario.Nombre}");
                    Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] 2FA activado: {usuario.DobleFactorActivado}");
                }

                Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Rol del usuario: {(usuario.Rol != null ? usuario.Rol.Nombre : "NULL")}");

                // 🔥 NUEVO: Verificar si tiene 2FA activado
                if (usuario.DobleFactorActivado)
                {
                    Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Usuario tiene 2FA activado, enviando código...");

                    // Generar código 2FA
                    var rng = new Random();
                    var codigo2FA = rng.Next(0, 1000000).ToString("D6");

                    // Enviar código por email
                    await EnviarCodigo2FAAsync(usuario.Email.Value, codigo2FA);

                    Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Código 2FA enviado: {codigo2FA}");

                    // 🔥 CORREGIDO: Usar propiedades existentes de RespuestaAuthDTO
                    return new RespuestaAuthDTO
                    {
                        Token = "", // Token vacío porque requiere 2FA
                        Usuario = new UsuarioInfoDTO
                        {
                            Id = usuario.Id,
                            Email = usuario.Email?.Value ?? string.Empty,
                            Nombre = usuario.Nombre ?? string.Empty,
                            RolId = usuario.Rol?.Id ?? 1,
                            RolNombre = usuario.Rol?.Nombre ?? "Usuario"
                        },
                        DobleFactorActivado = true, // Indicar que requiere 2FA
                    };
                }

                // 🔥 CONTINUACIÓN: Si no tiene 2FA, generar token directamente
                Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Generando token JWT...");
                var token = _jwtService.GenerarToken(usuario);
                Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] Token generado: {(string.IsNullOrEmpty(token) ? "VACÍO" : "PRESENTE")}");

                var usuarioInfo = new UsuarioInfoDTO
                {
                    Id = usuario.Id,
                    Email = usuario.Email?.Value ?? string.Empty,
                    Nombre = usuario.Nombre ?? string.Empty,
                    RolId = usuario.Rol?.Id ?? 1,
                    RolNombre = usuario.Rol?.Nombre ?? "Usuario"
                };

                Console.WriteLine($"🔵 [AUTH-SERVICE-SOCIAL] UsuarioInfoDTO creado - RolId: {usuarioInfo.RolId}, RolNombre: {usuarioInfo.RolNombre}");

                var respuesta = new RespuestaAuthDTO
                {
                    Token = token,
                    Usuario = usuarioInfo,
                    DobleFactorActivado = false // No requiere 2FA adicional
                };

                Console.WriteLine($"🟢 [AUTH-SERVICE-SOCIAL] LoginSocialAsync completado exitosamente");
                return respuesta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 [AUTH-SERVICE-SOCIAL] Error en LoginSocialAsync: {ex.Message}");
                Console.WriteLine($"🔴 [AUTH-SERVICE-SOCIAL] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        // ===============================
        // 📩 ENVÍO DE CÓDIGO 2FA
        // ===============================
        public async Task EnviarCodigo2FAAsync(string email, string codigo)
        {
            Console.WriteLine($"🔵 [AUTH-SERVICE-2FA] Enviando código 2FA a: {email}");

            var subject = "Código de verificación - CRM DocumentIA";
            var body = $@"
                <p>Tu código de verificación es: <strong>{codigo}</strong></p>
                <p>Expira en 5 minutos.</p>
                <p>Si no solicitaste este código, ignora este mensaje.</p>";

            await _smtpEmailService.SendEmailAsync(email, subject, body);

            Console.WriteLine($"🟢 [AUTH-SERVICE-2FA] Código 2FA enviado a: {email}");
        }
    }

    // 🔥 DTO PARA EL RESULTADO DEL REGISTRO
    public class RegistroResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Requires2FA { get; set; }
        public Guid? TwoFaTempId { get; set; }
    }
}