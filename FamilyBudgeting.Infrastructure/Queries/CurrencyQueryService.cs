using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Currencies;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class CurrencyQueryService : ICurrencyQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CurrencyQueryService> _logger;

        public CurrencyQueryService(IUnitOfWork unitOfWork, ILogger<CurrencyQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<CurrencyDto>> GetCurrenciesFromLedgerAsync()
        {
            string query = @"
                SELECT ""Id"",
                      ""Code"",
                      ""Symbol"",
                      ""Name"",
                      ""FractionalUnitFactor"",
                      ""IsActive"",
                      ""CreatedAt"",
                      ""UpdatedAt""
                FROM ""Currency""
                ORDER BY ""Name""
                ";
            _logger.LogQuery(query, null);

            return await _unitOfWork.Connection.QueryAsync<CurrencyDto>(query, null, _unitOfWork.Transaction);
        }

        public async Task<CurrencyDto?> GetCurrencyAsync(Guid id)
        {
            string query = @"
                SELECT ""Id"",
                      ""Code"",
                      ""Symbol"",
                      ""Name"",
                      ""FractionalUnitFactor"",
                      ""IsActive"",
                      ""CreatedAt"",
                      ""UpdatedAt""
                FROM ""Currency""
                WHERE ""Id"" = @Id
                ORDER BY ""Name""
                ";

            var qparams = new
            {
                Id = id,
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CurrencyDto>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<CurrencyDto?> GetCurrencyByCodeAsync(string code)
        {
            string query = @"
                SELECT ""Id"",
                      ""Code"",
                      ""Symbol"",
                      ""Name"",
                      ""FractionalUnitFactor"",
                      ""IsActive"",
                      ""CreatedAt"",
                      ""UpdatedAt""
                FROM ""Currency""
                WHERE ""Code"" = @Code
                ORDER BY ""Name""
                ";

            var qparams = new
            {
                Code = code,
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CurrencyDto>(query, qparams, _unitOfWork.Transaction);
        }
    }
}