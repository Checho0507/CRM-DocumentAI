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
            // 1. Obtener la clave secreta
            var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("Clave JWT no configurada.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 2. Definir los Claims (la información que irá en el token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email.Value), // ✅ Cambiado a .Value
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
                new Claim(ClaimTypes.Name, usuario.Nombre)
            };

            // 3. Crear las propiedades del token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"] ?? "1440")), // ✅ Valor por defecto
                signingCredentials: credentials);

            // 4. Escribir y devolver el token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}