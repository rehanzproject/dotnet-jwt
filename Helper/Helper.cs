// Helper/Helper.cs
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
namespace Helper.Helper
{
    public static class AuthHelper
    {
        private static string _secretKey;
        private static string _issuer;
        private static string _audience;


        static AuthHelper()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            _secretKey = config["JwtSettings:SecretKey"];
            _issuer = config["JwtSettings:Issuer"];
            _audience = config["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(_secretKey) || _secretKey.Length < 32)
            {
                throw new InvalidOperationException("The JWT secret key must be at least 32 characters long.");
            }
        }

        public static string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                
                // Add other claims as necessary
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string HashPassword(string password)
        {
            return new PasswordHasher<object>().HashPassword(null, password);
        }

        public static bool VerifyPasswordHash(string providedPassword, string storedHash)
        {
            var verificationResult = new PasswordHasher<object>().VerifyHashedPassword(null, storedHash, providedPassword);
            return verificationResult == PasswordVerificationResult.Success;
        }
    }
}
