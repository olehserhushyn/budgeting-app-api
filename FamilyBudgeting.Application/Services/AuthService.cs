using Ardalis.Result;
using FamilyBudgeting.Domain.Data.Users;
using FamilyBudgeting.Domain.DTOs.Requests.Auths;
using FamilyBudgeting.Domain.DTOs.Responses.Auths;
using FamilyBudgeting.Domain.Exceptions;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Infrastructure.JwtProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace FamilyBudgeting.Domain.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtProvider _jwtProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _memoryCache;
        private readonly IIdentityRepository _identityRepository;
        private readonly IUserSetupService _userSetupService;
        private readonly IUserSettingsService _userSettingsService;

        public AuthService(IJwtProvider jwtProvider, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor,
            IEmailService emailService, IMemoryCache memoryCache, IIdentityRepository identityRepository,
            IUserSetupService userSetupService, IUserSettingsService userSettingsService)
        {
            _jwtProvider = jwtProvider;
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _memoryCache = memoryCache;
            _identityRepository = identityRepository;
            _userSetupService = userSetupService;
            _userSettingsService = userSettingsService;
        }

        public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Email.ToLower());

            if (user is null || user.IsDeleted)
            {
                return Result.Error("User not found.");
            }

            if (!user.EmailConfirmed)
            {
                // --- Begin: Email verification logic with rate limiting ---
                var email = user.Email.ToLower();
                var cacheKey2Min = $"login-verification-2min:{email}";
                var cacheKeyDay = $"login-verification-day:{email}";
                var now = DateTime.UtcNow;

                // 2-min rate limit
                if (_memoryCache.TryGetValue(cacheKey2Min, out _))
                {
                    // Do not send another email
                }
                else
                {
                    _memoryCache.Set(cacheKey2Min, true, TimeSpan.FromMinutes(2));

                    // Daily limit
                    int sentToday = 0;
                    if (_memoryCache.TryGetValue(cacheKeyDay, out int count))
                    {
                        sentToday = count;
                    }
                    if (sentToday < 5)
                    {
                        sentToday++;
                        _memoryCache.Set(cacheKeyDay, sentToday, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTimeOffset.UtcNow.Date.AddDays(1) // expires at midnight UTC
                        });
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var origin = "http://localhost:5173"; // TODO: pass real origin if available
                        var link = $"{origin}/verify-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
                        await _emailService.SendVerificationEmailAsync(user.Email, link);
                    }
                    else
                    {
                        // Daily limit reached, do not send email
                        return Result.Forbidden("EMAIL_VERIFICATION_DAILY_LIMIT");
                    }
                }
                // --- End: Email verification logic with rate limiting ---
                return Result.Forbidden("EMAIL_NOT_CONFIRMED");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                _jwtProvider.GenerateTokenAndSetCookie(user, roles?.ToList() ?? new List<string>(), _httpContextAccessor.HttpContext);

                // Include user settings in the response if available
                var userSettingsResult = await _userSettingsService.GetUserSettingsAsync(user.Id);

                return Result.Success(new LoginResponse
                {
                    LoginSuccess = true,
                    RequiresTwoFactor = false,
                    User = new UserAuthResponse
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Roles = roles?.ToArray() ?? new string[0],
                        EmailConfirmed = user.EmailConfirmed,
                        UserSettings = userSettingsResult.IsSuccess ? userSettingsResult.Value : null
                    }
                });
            }

            return Result.Error("Invalid password.");
        }

        public async Task<Result<string>> RegisterAsync(string firstName, string lastName, string email, string password)
        {
            string emailLink = string.Empty;
            try
            {
                await CheckIfPasswordValid(password, password);

                var user = await _identityRepository.RegisterAsync(firstName, lastName, email, password);
                if (user is null)
                    return Result.Error("Unexpected error registering the user");

                // Create default UserSettings, ledger, and account
                var setupResult = await _userSetupService.SetupDefaultUserDataAsync(user.Id, firstName);
                if (!setupResult.IsSuccess)
                {
                    var setupErrors = setupResult.Errors != null && setupResult.Errors.Any()
                        ? string.Join(" ", setupResult.Errors)
                        : "Unknown error";
                    return Result.Error($"User created but failed to setup default data: {setupErrors}");
                }

                var token = await _identityRepository.GenerateEmailConfirmationTokenAsync(user);
                var origin = "http://localhost:5173"; // TODO: Use configuration
                emailLink = $"{origin}/verify-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            }
            catch (DomainValidationException ex)
            {
                return Result.Error(ex.Message);
            }
            
            try
            {
                await _emailService.SendVerificationEmailAsync(email, emailLink);
                return Result.Success("Registration successful. Please check your email to verify your account.");
            }
            catch (Exception ex)
            {
                return Result.Error($"Registration failed: Unable to send verification email. Error: {ex.Message}");
            }
        }

        public async Task<Result> SendEmailVerificationAsync(string email, string origin)
        {
            var cacheKey = $"resend-verification:{email.ToLower()}";
            if (_memoryCache.TryGetValue(cacheKey, out _))
                return Result.Error("You can only request a verification email once per minute. Please wait and try again.");
            _memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(1));
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Error("User not found.");
            if (user.EmailConfirmed)
                return Result.Error("Email already confirmed.");
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = $"{origin}/verify-email?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendVerificationEmailAsync(user.Email, link);
            return Result.Success();
        }

        public async Task<Result<LoginResponse>> VerifyEmailAsync(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Error("User not found.");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Auto-login: set JWT cookie and return user info
                var roles = await _userManager.GetRolesAsync(user);
                _jwtProvider.GenerateTokenAndSetCookie(user, roles?.ToList() ?? new List<string>(), _httpContextAccessor.HttpContext);
                return Result.Success(new LoginResponse
                {
                    LoginSuccess = true,
                    RequiresTwoFactor = false,
                    User = new UserAuthResponse
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        EmailConfirmed = user.EmailConfirmed,
                        Roles = roles?.ToArray() ?? new string[0]
                    }
                });
            }
            return Result.Error(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task<Result> SendPasswordResetEmailAsync(string email, string origin)
        {
            var cacheKey = $"resend-reset:{email.ToLower()}";
            if (_memoryCache.TryGetValue(cacheKey, out _))
                return Result.Error("You can only request a password reset email once per minute. Please wait and try again.");
            _memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(1));
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Result.Error("User not found.");
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = $"{origin}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, link);
            return Result.Success();
        }

        public async Task<Result> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            if (!Guid.TryParse(userId, out var guid))
                return Result.Error("Invalid user id.");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Error("User not found.");
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
                return Result.Success();
            return Result.Error(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task<Result> UpdateProfileAsync(string userId, string firstName, string lastName)
        {
            if (!Guid.TryParse(userId, out var guid))
                return Result.Error("Invalid user id.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Error("User not found.");

            user.UpdateProfile(firstName, lastName);

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Result.Success();

            return Result.Error(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            if (!Guid.TryParse(userId, out var guid))
                return Result.Error("Invalid user id.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Result.Error("User not found.");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
                return Result.Success();

            return Result.Error(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        private async Task CheckIfPasswordValid(string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                throw new DomainValidationException("Password and Confirm Password must match.");
            }
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, null, password);

                if (!result.Succeeded)
                {
                    var descriptions = string.Join("", result.Errors.Select(e => e.Description + Environment.NewLine));
                    throw new DomainValidationException(descriptions);
                }
            }
        }
    }
}
