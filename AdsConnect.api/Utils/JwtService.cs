using AdsConnect.data.Dtos;
using AdsConnect.data.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AdsConnect.api.Utils
{
    /// <summary>
    /// Issues the access token. Mirrors PMS's Member.api.Utils.JwtService, minus the
    /// ASP.NET Identity UserManager - AdsConnect keeps its own AppUser/Role/UserRole
    /// tables, so the roles are passed in rather than looked up here.
    /// </summary>
    public class JwtService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string GenerateToken(AuthUserDto user)
        {
            var fullName = string.IsNullOrWhiteSpace(user.lastName)
                ? user.firstName
                : $"{user.firstName} {user.lastName}";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("name", fullName),
            };

            // JwtSecurityTokenHandler's outbound map shortens ClaimTypes.Role to "role",
            // and the inbound map on the validating side turns it back again, so
            // [Authorize(Roles = "Admin")] works against these.
            foreach (var roleName in user.roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_jwtSettings.Lifetime),
                SigningCredentials = new SigningCredentials(_jwtSettings.SigningKey, SecurityAlgorithms.HmacSha256),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}
