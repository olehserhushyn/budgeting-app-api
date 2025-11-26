using FamilyBudgeting.Domain.DTOs.Models.Currencies;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface ICurrencyQueryService
    {
        Task<IEnumerable<CurrencyDto>> GetCurrenciesFromLedgerAsync();
        Task<CurrencyDto?> GetCurrencyAsync(Guid id);
        Task<CurrencyDto?> GetCurrencyByCodeAsync(string code);
    }
}
