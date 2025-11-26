namespace FamilyBudgeting.Domain.DTOs.Requests.Accounts
{
    public record UpdateAccountRequest(Guid AccountId, Guid AccountTypeId, string Title, int Balance, Guid CurrencyId);
}
