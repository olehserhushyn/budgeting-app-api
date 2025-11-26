namespace FamilyBudgeting.Domain.DTOs.Requests.Auths
{
    public record SendPasswordResetRequest(string Email, string Origin);
} 