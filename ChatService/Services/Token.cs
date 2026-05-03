using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatService.Models;

namespace ChatService.Services
{
    public class Token
    {
        private readonly IOptions<JWTConf> _configuration;
        public Token(IOptions<JWTConf> configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// Generates jwt token for user with given ID
        /// </summary>
        /// <param name="email"></param>
        /// <param name="userId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public string GenerateJwtToken(string email, string userId, UserRole role = UserRole.User)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.JWTSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId), // Context.UserIdentifier
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration.Value.JWTValidIssuer,
                audience: _configuration.Value.JWTValidAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(BusinessSettings.tokenExpiration.TotalHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
