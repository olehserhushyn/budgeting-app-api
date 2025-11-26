using FamilyBudgeting.Domain.Data.Users;
using Microsoft.AspNetCore.Http;

namespace FamilyBudgeting.Infrastructure.JwtProviders
{
    public interface IJwtProvider
    {
        string GenerateToken(ApplicationUser user, List<string> roles);
        void GenerateTokenAndSetCookie(ApplicationUser user, List<string> roles, HttpContext httpContext);
    }
}
