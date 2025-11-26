using FamilyBudgeting.Domain.Services.Interfaces;
using Microsoft.Extensions.Options;
using Resend;
using System.Threading.Tasks;

namespace FamilyBudgeting.Infrastructure
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly string _defaultFrom;

        public EmailService(IResend resend, IOptions<ResendClientOptions> options)
        {
            _resend = resend;
            _defaultFrom = "onboarding@resend.dev";
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, string? from = null)
        {
            var message = new EmailMessage
            {
                From = from ?? _defaultFrom,
                Subject = subject,
                HtmlBody = htmlBody
            };
            message.To.Add(to);
            await _resend.EmailSendAsync(message);
        }

        public async Task SendVerificationEmailAsync(string to, string verificationLink)
        {
            var subject = "Verify your email address - Family Budgeting App";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; border: 1px solid #eee; border-radius: 8px; padding: 24px;'>
                    <h2 style='color: #1976d2;'>Welcome to Family Budgeting App!</h2>
                    <p>Thank you for registering. Please verify your email address to activate your account.</p>
                    <p style='text-align: center; margin: 32px 0;'>
                        <a href='{verificationLink}' style='background: #1976d2; color: #fff; padding: 12px 24px; border-radius: 4px; text-decoration: none; font-weight: bold;'>Verify Email</a>
                    </p>
                    <p>If you did not create an account, you can safely ignore this email.</p>
                    <hr style='margin: 24px 0;'>
                    <p style='font-size: 12px; color: #888;'>If you have any questions, contact our support at <a href='mailto:support@yourdomain.com'>support@yourdomain.com</a>.</p>
                </div>";
            await SendEmailAsync(to, subject, htmlBody);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Reset your password - Family Budgeting App";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; border: 1px solid #eee; border-radius: 8px; padding: 24px;'>
                    <h2 style='color: #1976d2;'>Reset Your Password</h2>
                    <p>We received a request to reset your password. Click the button below to set a new password.</p>
                    <p style='text-align: center; margin: 32px 0;'>
                        <a href='{resetLink}' style='background: #1976d2; color: #fff; padding: 12px 24px; border-radius: 4px; text-decoration: none; font-weight: bold;'>Reset Password</a>
                    </p>
                    <p>If you did not request a password reset, you can safely ignore this email.</p>
                    <hr style='margin: 24px 0;'>
                    <p style='font-size: 12px; color: #888;'>If you have any questions, contact our support at <a href='mailto:support@yourdomain.com'>support@yourdomain.com</a>.</p>
                </div>";
            await SendEmailAsync(to, subject, htmlBody);
        }
    }
} 