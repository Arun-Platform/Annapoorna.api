using Annapurnaworld.data;
using Annapurnaworld.entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Annapurnaworld.service
{
    /// <summary>
    /// Class to handle token generation and validation.
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Contructor to initialize the data.
        /// </summary>
        /// <param name="configuration">configuration</param>
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtSettings = new JwtSettings();
            _jwtSettings.Issuer = _configuration["jwt_key:Issuer"].ToString();
            _jwtSettings.Audience = _configuration["jwt_key:Audience"].ToString();
            _jwtSettings.Secret = _configuration["jwt_key:Secret"].ToString();
            _jwtSettings.ExpirationMinutes = int.Parse(_configuration["jwt_key:ExpirationMinutes"].ToString());
            _jwtSettings.ExpirationMinutesForRefreshToken = int.Parse(_configuration["jwt_key:ExpirationMinutesForRefreshToken"].ToString());
        }

        /// <summary>
        /// Generate JWT token
        /// </summary>
        /// <param name="userId">userid</param>
        /// <returns>token</returns>
        public string GenerateJwtToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor();
            var Subject = new ClaimsIdentity(new[]
             {
                new Claim(ClaimTypes.NameIdentifier, userId)
            });
            tokenDescriptor.Subject = Subject;
            tokenDescriptor.Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
            tokenDescriptor.Issuer = _jwtSettings.Issuer;
            tokenDescriptor.Audience = _jwtSettings.Audience;
            tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            try
            {
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Generate refresh token
        /// </summary>
        /// <param name="userId">userid</param>
        /// <returns>refresh token</returns>
        public string GenerateRefreshToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutesForRefreshToken),
                Issuer = _jwtSettings.Issuer,
                Subject = new ClaimsIdentity(new[]
                {
                      new Claim(ClaimTypes.NameIdentifier, userId)
                }),
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            try
            {
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Validate refresh token.
        /// </summary>
        /// <param name="refreshToken">refresh token</param>
        /// <returns>userid</returns>
        public Guid ValidateRefreshToken(string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out SecurityToken validatedToken);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null)
                {
                    return Guid.Parse(userIdClaim);
                }
                throw new UnauthorizedAccessException();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
