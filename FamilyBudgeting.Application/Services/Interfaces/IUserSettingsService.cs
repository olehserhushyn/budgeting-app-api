using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Users;
using FamilyBudgeting.Domain.DTOs.Requests.Users;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IUserSettingsService
    {
        Task<Result<UserSettingsDto>> GetUserSettingsAsync(Guid userId);
        Task<Result> UpdateUserSettingsAsync(Guid userId, UpdateUserSettingsRequest request);
    }
}
