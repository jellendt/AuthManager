using AuthManager.Entities;
using AuthManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthManager.Services.JwtService
{
    public class JwtService(
        IOptions<JwtOptions> jwtOptions) : IJwtService
    {
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;

        public string GenerateJwtToken(User user)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            SecurityTokenDescriptor tokenDescriptor = BuildTokenDescriptor(user);
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Guid? GetGuidIdFromClaims(List<Claim> claims)
        {
            if (Guid.TryParse(claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value, out Guid userGuid))
            {
                return userGuid;
            }
            return null;
        }

        private SecurityTokenDescriptor BuildTokenDescriptor(User user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_jwtOptions.Key);
            return new()
            {
                Subject = new ClaimsIdentity([
                    new Claim(JwtRegisteredClaimNames.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.EMail),
                    new Claim("role", user.Role.ToString()) ]),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience
            };
        }
    }
}
