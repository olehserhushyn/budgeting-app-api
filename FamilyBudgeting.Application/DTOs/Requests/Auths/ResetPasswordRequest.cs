namespace FamilyBudgeting.Domain.DTOs.Requests.Auths
{
    public record ResetPasswordRequest(string UserId, string Token, string NewPassword);
} 