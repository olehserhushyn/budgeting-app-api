using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Auths;
using FamilyBudgeting.Domain.DTOs.Responses.Auths;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<string>> RegisterAsync(string firstName, string lastName, string email, string password);
        Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
        Task<Result> SendEmailVerificationAsync(string email, string origin);
        Task<Result<LoginResponse>> VerifyEmailAsync(string email, string token);
        Task<Result> SendPasswordResetEmailAsync(string email, string origin);
        Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);
        Task<Result> UpdateProfileAsync(string userId, string firstName, string lastName);
        Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
