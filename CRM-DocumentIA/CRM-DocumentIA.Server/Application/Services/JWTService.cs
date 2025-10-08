// Application/Services/JwtService.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CRM_DocumentIA.Server.Domain.Entities; // Para usar la entidad Usuario

namespace CRM_DocumentIA.Server.Application.Services
{
    // Clase renombrada a JwtService
    public class JWTService
    {
        private readonly IConfiguration _configuration;

        public JWTService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Método renombrado a GenerarToken (siguiendo el estilo original de su proyecto)
        public string GenerarToken(Usuario usuario)
        {
            // 1. Obtener la clave secreta
            var secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("Clave JWT no configurada.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 2. Definir los Claims (la información que irá en el token)
            var claims = new[]
            {
                // Este claim es crucial para el ObtenerUsuarioId() en el controlador
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email.Valor),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(ClaimTypes.Name, usuario.Nombre)
            };

            // 3. Crear las propiedades del token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
                signingCredentials: credentials);

            // 4. Escribir y devolver el token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}