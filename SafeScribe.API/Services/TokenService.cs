using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SafeScribe.API.Interfaces;
using SafeScribe.API.Models;


namespace SafeScribe.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Configuração Jwt:Key não encontrada.");
            var issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Configuração Jwt:Issuer não encontrada.");
            var audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("Configuração Jwt:Audience não encontrada.");
            var expireMinutesString = _config["Jwt:ExpireMinutes"] ?? "60";

            if (!double.TryParse(expireMinutesString, out var expireMinutes))
            {
                expireMinutes = 60;
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}