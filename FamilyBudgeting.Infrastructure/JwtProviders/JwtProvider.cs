using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FamilyBudgeting.Infrastructure.JwtProviders
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtOptions _jwtOptions;

        public JwtProvider(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public string GenerateToken(ApplicationUser user, List<string> roles)
        {
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                //new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // Correct Subject Claim
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),        // Email
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique Token ID
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),    // Name Identifier
                new Claim(ClaimTypes.Name, user.FirstName),                    // User's Display Name
                new Claim(ClaimTypes.Email, user.Email),                      // User's Email
                new Claim("email_confirmed", user.EmailConfirmed.ToString())
            };

            // Add roles to JWT if the user has any
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));  // Adding role claims
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,     // Ensure Issuer is included
                audience: _jwtOptions.Audience, // Ensure Audience is included
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpiresHours),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public void GenerateTokenAndSetCookie(ApplicationUser user, List<string> roles, HttpContext httpContext)
        {
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("email_confirmed", user.EmailConfirmed.ToString())
            };

            // Add roles to JWT
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtOptions.ExpiresHours),
                signingCredentials: signingCredentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Set cookie options
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = httpContext.Request.IsHttps, // Only set Secure if using HTTPS
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(_jwtOptions.ExpiresHours),
                Path = "/", // Make sure the cookie is accessible across all paths
                Domain = null // Let the browser set the domain automatically
            };

            // Add the cookie to the response
            httpContext.Response.Cookies.Append(AppConstants.JwtCookieName, tokenString, cookieOptions);
            
            // logger?.LogDebug("Set JWT cookie '{CookieName}' with options: HttpOnly={HttpOnly}, Secure={Secure}, SameSite={SameSite}, Path={Path}, Domain={Domain}", 
            //     AppConstants.JwtCookieName, cookieOptions.HttpOnly, cookieOptions.Secure, cookieOptions.SameSite, cookieOptions.Path, cookieOptions.Domain);
        }
    }
}
