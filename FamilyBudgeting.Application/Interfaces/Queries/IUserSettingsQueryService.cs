using FamilyBudgeting.Domain.DTOs.Models.Users;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IUserSettingsQueryService
    {
        Task<UserSettingsDto?> GetUserSettingsAsync(Guid userId);
    }
}
