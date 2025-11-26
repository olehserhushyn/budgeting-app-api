namespace FamilyBudgeting.Domain.Data.UsersSettings
{
    public interface IUserSettingsRepository
    {
        Task<Guid> CreateUserSettingsAsync(UserSettings userSettings);
        Task<bool> UpdateUserSettingsAsync(Guid settingsId, UserSettings userSettings);
    }
}
