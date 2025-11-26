using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Requests.Auths
{
    public record LoginRequest(
        string Email,
        string Password,
        bool RememberMe = false
    );
}
