namespace FamilyBudgeting.Domain.DTOs.Requests.Users
{
    public record UpdateUserSettingsRequest(
        Guid MainCurrencyId,
        bool ShowOnboarding
    );
}
