using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Be.Models;

namespace Be.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        // Generates a signed JWT token for the given account
        public string GenerateToken(Account account)
        {
            // Prepare claim list that includes ID, email, and role
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Role, account.Role)
            };

            // Load symmetric signing key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build the JWT token with issuer, audience, expiration and claims
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],             // e.g., https://yourdomain.com
                audience: _config["Jwt:Audience"],         // e.g., https://yourdomain.com
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),      // Token expiration duration
                signingCredentials: creds
            );

            // Return the encoded token string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
