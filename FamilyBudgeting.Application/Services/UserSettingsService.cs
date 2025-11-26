using Ardalis.Result;
using FamilyBudgeting.Domain.Data.UsersSettings;
using FamilyBudgeting.Domain.DTOs.Requests.Users;
using FamilyBudgeting.Domain.DTOs.Models.Users;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Interfaces.Queries;

namespace FamilyBudgeting.Domain.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsRepository _userSettingsRepository;
        private readonly IUserSettingsQueryService _userSettingsQueryService;

        public UserSettingsService(IUserSettingsRepository userSettingsRepository, IUserSettingsQueryService userSettingsQueryService)
        {
            _userSettingsRepository = userSettingsRepository;
            _userSettingsQueryService = userSettingsQueryService;
        }

        public async Task<Result<UserSettingsDto>> GetUserSettingsAsync(Guid userId)
        {
            var userSettings = await _userSettingsQueryService.GetUserSettingsAsync(userId);
            if (userSettings == null)
            {
                return Result<UserSettingsDto>.NotFound("User settings not found");
            }

            return Result.Success(userSettings);
        }

        public async Task<Result> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingsRequest request)
        {
            var existingSettings = await _userSettingsQueryService.GetUserSettingsAsync(userId);
            if (existingSettings == null)
            {
                return Result.NotFound("User settings not found");
            }

            var settings = new UserSettings(existingSettings.UserId, existingSettings.MainCurrencyId, existingSettings.ShowOnboarding);

            settings.Update(request.MainCurrencyId, request.ShowOnboarding);
            var updated = await _userSettingsRepository.UpdateUserSettingsAsync(existingSettings.Id, settings);
            
            if (!updated)
            {
                return Result.Error("Failed to update user settings");
            }

            return Result.Success();
        }
    }
}
