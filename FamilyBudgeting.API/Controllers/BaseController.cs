using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyBudgeting.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid GetUserIdFromToken()
        {
            // Extract the user ID claim
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID not found in token.");
            }

            // Validate the claim value as an Guid
            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User ID in token is invalid.");
            }

            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User ID in token is invalid.");
            }

            return userId;
        }
    }
}
