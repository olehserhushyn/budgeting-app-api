using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Auths;
using FamilyBudgeting.Domain.DTOs.Responses.Auths;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            return (await _authService.RegisterAsync(request.FirstName, 
                request.LastName, request.Email, request.Password)).ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            return (await _authService.LoginAsync(request)).ToActionResult();
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AppConstants.JwtCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Include user settings when returning current user
            var userSettingsService = HttpContext.RequestServices.GetService(typeof(FamilyBudgeting.Domain.Services.Interfaces.IUserSettingsService)) as FamilyBudgeting.Domain.Services.Interfaces.IUserSettingsService;
            var userSettings = null as FamilyBudgeting.Domain.DTOs.Models.Users.UserSettingsDto;
            if (userSettingsService != null)
            {
                var s = await userSettingsService.GetUserSettingsAsync(user.Id);
                if (s.IsSuccess)
                    userSettings = s.Value;
            }

            return Ok(new UserAuthResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles?.ToArray() ?? new string[0],
                EmailConfirmed = user.EmailConfirmed,
                UserSettings = userSettings
            });
        }

        [AllowAnonymous]
        [HttpPost("send-email-verification")]
        public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationRequest request)
        {
            return (await _authService.SendEmailVerificationAsync(request.Email, request.Origin)).ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            return (await _authService.VerifyEmailAsync(request.Email, request.Token)).ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("send-password-reset")]
        public async Task<IActionResult> SendPasswordReset([FromBody] SendPasswordResetRequest request)
        {
            return (await _authService.SendPasswordResetEmailAsync(request.Email, request.Origin)).ToActionResult();
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request.UserId, request.Token, request.NewPassword);
            if (result.IsSuccess)
            {
                return Redirect($"/reset-password-result?success=1");
            }
            else
            {
                var errorMsg = Uri.EscapeDataString(result.Errors.FirstOrDefault() ?? "Password reset failed.");
                return Redirect($"/reset-password-result?error={errorMsg}");
            }
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            return (await _authService.UpdateProfileAsync(userId, request.FirstName, request.LastName)).ToActionResult();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { Message = "New password and confirm password do not match." });
            }

            return (await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword)).ToActionResult();
        }
    }
}
