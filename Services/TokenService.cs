using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoyaltyRewardsApi.Data;
using LoyaltyRewardsApi.DTOs;
using LoyaltyRewardsApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoyaltyRewardsApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly LoyaltyRewardsContext _context;

        public TokenService(IConfiguration configuration, LoyaltyRewardsContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<string> GenerateJwtToken(UserDto user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            var key = Encoding.ASCII.GetBytes(secretKey);
            var issuer = jwtSettings["Issuer"] ?? "loyalty-rewards-api";
            var audience = jwtSettings["Audience"] ?? "loyalty-rewards-app";
            var expiryMinutesStr = jwtSettings["ExpiryMinutes"] ?? "1440";
            var expiryMinutes = int.Parse(expiryMinutesStr);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("IsActive", user.IsActive.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // Store session in database
            var tokenHash = BCrypt.Net.BCrypt.HashPassword(tokenString);
            var session = new UserSession
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = tokenDescriptor.Expires.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return tokenString;
        }

        public ClaimsPrincipal? ValidateJwtToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
                var key = Encoding.ASCII.GetBytes(secretKey);
                var issuer = jwtSettings["Issuer"] ?? "loyalty-rewards-api";
                var audience = jwtSettings["Audience"] ?? "loyalty-rewards-app";

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> IsTokenValidInDatabase(string token, int userId)
        {
            try
            {
                var sessions = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive && s.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    if (BCrypt.Net.BCrypt.Verify(token, session.TokenHash))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task RevokeToken(string token, int userId)
        {
            try
            {
                var sessions = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive)
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    if (BCrypt.Net.BCrypt.Verify(token, session.TokenHash))
                    {
                        session.IsActive = false;
                        _context.UserSessions.Update(session);
                        break;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch
            {
                // Log error if needed
            }
        }

        public async Task RevokeAllUserTokens(int userId)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            _context.UserSessions.UpdateRange(sessions);
            await _context.SaveChangesAsync();
        }

        public async Task CleanupExpiredTokens()
        {
            var expiredSessions = await _context.UserSessions
                .Where(s => s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.UserSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();
        }
    }

    public interface ITokenService
    {
        Task<string> GenerateJwtToken(UserDto user);
        ClaimsPrincipal? ValidateJwtToken(string token);
        Task<bool> IsTokenValidInDatabase(string token, int userId);
        Task RevokeToken(string token, int userId);
        Task RevokeAllUserTokens(int userId);
        Task CleanupExpiredTokens();
    }
}
