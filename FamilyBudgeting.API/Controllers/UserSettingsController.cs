using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Users;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class UserSettingsController : BaseController
    {
        private readonly IUserSettingsService _userSettingsService;

        public UserSettingsController(IUserSettingsService userSettingsService)
        {
            _userSettingsService = userSettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSettings()
        {
            Guid userId = GetUserIdFromToken();
            return (await _userSettingsService.GetUserSettingsAsync(userId)).ToActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserSettings(UpdateUserSettingsRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _userSettingsService.UpdateUserSettingsAsync(userId, request)).ToActionResult();
        }
    }
}
