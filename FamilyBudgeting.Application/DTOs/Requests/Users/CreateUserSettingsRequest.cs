namespace FamilyBudgeting.Domain.DTOs.Requests.Users
{
    public record CreateUserSettingsRequest(
        Guid MainCurrencyId,
        bool ShowOnboarding
    );
}
