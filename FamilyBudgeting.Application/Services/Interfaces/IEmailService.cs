using System.Threading.Tasks;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlBody, string? from = null);
        Task SendVerificationEmailAsync(string to, string verificationLink);
        Task SendPasswordResetEmailAsync(string to, string resetLink);
    }
} 