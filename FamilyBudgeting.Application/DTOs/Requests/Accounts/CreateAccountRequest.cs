namespace FamilyBudgeting.Domain.DTOs.Requests.Accounts
{
    public record CreateAccountRequest(Guid AccountTypeId, string Title, int Balance, Guid CurrencyId);
}
