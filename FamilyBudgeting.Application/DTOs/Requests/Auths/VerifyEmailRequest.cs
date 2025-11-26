namespace FamilyBudgeting.Domain.DTOs.Requests.Auths
{
    public record VerifyEmailRequest(string Email, string Token);
} 