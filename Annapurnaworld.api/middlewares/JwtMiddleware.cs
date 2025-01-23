using Annapurnaworld.entity;
using Annapurnaworld.service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Annapurnaworld.api
{
    /// <summary>
    /// Middleware to handle JWt Authentication.
    /// </summary>
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;
        private readonly string _secretKey;

        /// <summary>
        /// Constructor to initalize the data.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="_configuration"></param>
        public JwtMiddleware(RequestDelegate next, IConfiguration _configuration)
        {
            _next = next;
            _jwtSettings = new JwtSettings();
            _jwtSettings.Issuer = _configuration["jwt_key:Issuer"].ToString();
            _jwtSettings.Audience = _configuration["jwt_key:Audience"].ToString();
            _jwtSettings.Secret = _configuration["jwt_key:Secret"].ToString();
            _jwtSettings.ExpirationMinutes = int.Parse(_configuration["jwt_key:ExpirationMinutes"].ToString());
            _jwtSettings.ExpirationMinutesForRefreshToken = int.Parse(_configuration["jwt_key:ExpirationMinutesForRefreshToken"].ToString());
        }

        /// <summary>
        /// Handles validation of the jwt token.
        /// </summary>
        /// <param name="context">http contect</param>
        /// <param name="userService">userservice</param>
        /// <returns>void</returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            if (context.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = _jwtSettings.Issuer,
                        ValidAudience = _jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };

                    // Validate the token and get the claims
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                    var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim != null)
                    {
                        var currentUser = await userService.GetUserById(Guid.Parse(userIdClaim));
                        var userIdentity = new GenericIdentity(currentUser.FullName);
                        var claimsPrincipal = new GenericPrincipal(userIdentity, currentUser.UserRoles.Select(x => x.Role.Name).ToArray());
                        context.User = claimsPrincipal;
                        context.Items["UserInfo"] = currentUser;
                        await _next(context);
                    }
                }
                catch(UnauthorizedAccessException ex)
                {
                    throw;
                }
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
