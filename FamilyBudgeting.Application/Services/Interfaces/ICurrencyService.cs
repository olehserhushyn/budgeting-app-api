using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Responses.Currencies;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<Result<IEnumerable<GetCurrenciesResponse>>> GetCurrenciesFromLedgerAsync();
    }
}
