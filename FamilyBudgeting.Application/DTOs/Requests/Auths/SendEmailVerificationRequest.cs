namespace FamilyBudgeting.Domain.DTOs.Requests.Auths
{
    public record SendEmailVerificationRequest(string Email, string Origin);
} 