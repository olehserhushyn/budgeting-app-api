using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Responses.Currencies;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyQueryService _currencyQueryService;

        public CurrencyService(ICurrencyQueryService currencyQueryService)
        {
            _currencyQueryService = currencyQueryService;
        }

        public async Task<Result<IEnumerable<GetCurrenciesResponse>>> GetCurrenciesFromLedgerAsync()
        {
            var currencies = await _currencyQueryService.GetCurrenciesFromLedgerAsync();
            return Result.Success(currencies.Select(x => new GetCurrenciesResponse
            {
                Id = x.Id,
                Code = x.Code,
                Symbol = x.Symbol,
                Name = x.Name,
                FractionalUnitFactor = x.FractionalUnitFactor
            }));
        }
    }
}
