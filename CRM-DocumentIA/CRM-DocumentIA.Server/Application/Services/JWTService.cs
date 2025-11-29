using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using CRM_DocumentIA.Server.Domain.Entities;

namespace CRM_DocumentIA.Server.Application.Services
{
    public class JWTService
    {
        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(Usuario usuario)
        {
            Console.WriteLine("🔵 [JWTService] INICIO - GenerarToken");

            try
            {
                // 🔥 VALIDACIÓN 1: Verificar que el usuario no sea null
                if (usuario == null)
                {
                    Console.WriteLine("🔴 [JWTService] ERROR: usuario es null");
                    throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo");
                }

                Console.WriteLine($"🔵 [JWTService] Procesando usuario: {usuario.Nombre}, ID: {usuario.Id}");

                // 🔥 VALIDACIÓN 2: Verificar configuración JWT
                var secretKey = _configuration["JwtSettings:Secret"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    Console.WriteLine("🔴 [JWTService] ERROR: JwtSettings:Secret no configurado");
                    throw new InvalidOperationException("Clave JWT no configurada.");
                }

                Console.WriteLine($"🔵 [JWTService] Configuración JWT encontrada");

                // 🔥 VALIDACIÓN 3: Verificar propiedades críticas del usuario
                if (usuario.Email == null)
                {
                    Console.WriteLine("🔴 [JWTService] ERROR: usuario.Email es null");
                    throw new InvalidOperationException("El usuario no tiene email asignado.");
                }

                Console.WriteLine($"🔵 [JWTService] Email del usuario: {usuario.Email.Value}");

                // 🔥 VALIDACIÓN 4: Manejar el caso cuando Rol es null
                string rolNombre = "Usuario"; // Valor por defecto

                if (usuario.Rol != null)
                {
                    rolNombre = usuario.Rol.Nombre ?? "Usuario";
                    Console.WriteLine($"🔵 [JWTService] Rol del usuario: {rolNombre}");
                }
                else
                {
                    Console.WriteLine($"⚠️ [JWTService] ADVERTENCIA: usuario.Rol es null, usando rol por defecto: {rolNombre}");
                    // No lanzamos excepción, usamos valor por defecto para no bloquear el login
                }

                // 1. Obtener la clave secreta
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // 2. Definir los Claims (la información que irá en el token)
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email.Value),
                    new Claim(ClaimTypes.Role, rolNombre), // ✅ Usamos el rol manejado seguramente
                    new Claim(ClaimTypes.Name, usuario.Nombre ?? string.Empty),
                    new Claim("DobleFactorActivado", usuario.DobleFactorActivado.ToString().ToLower())
                };

                Console.WriteLine($"🔵 [JWTService] Claims creados - ID: {usuario.Id}, Email: {usuario.Email.Value}, Rol: {rolNombre}");

                // 3. Crear las propiedades del token con valores por defecto
                var issuer = _configuration["JwtSettings:Issuer"] ?? "CRM-DocumentIA";
                var audience = _configuration["JwtSettings:Audience"] ?? "CRM-DocumentIA-Users";
                var expiryMinutes = _configuration["JwtSettings:ExpiryMinutes"] ?? "1440";

                Console.WriteLine($"🔵 [JwtSettings] Issuer: {issuer}, Audience: {audience}, ExpiryMinutes: {expiryMinutes}");

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(expiryMinutes)),
                    signingCredentials: credentials);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                Console.WriteLine($"🟢 [JWTService] Token generado exitosamente");
                return tokenString;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 [JWTService] ERROR: {ex.Message}");
                Console.WriteLine($"🔴 [JWTService] StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}