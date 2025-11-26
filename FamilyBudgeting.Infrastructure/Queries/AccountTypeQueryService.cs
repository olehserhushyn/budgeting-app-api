using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.AccountTypes;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class AccountTypeQueryService : IAccountTypeQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountTypeQueryService> _logger;

        public AccountTypeQueryService(IUnitOfWork unitOfWork, ILogger<AccountTypeQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<AccountTypeDto?> GetAccountTypeEntity(Guid accountTypeId)
        {
            string query = @"
                SELECT ""Id"", ""Title""
                FROM ""AccountType""
                WHERE ""Id"" = @AccountTypeId;
                ";
            var qparams = new
            {
                AccountTypeId = accountTypeId
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountTypeDto>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<AccountTypeDto>> GetAccountTypes()
        {
            string query = @"
                SELECT ""Id"", ""Title""
                FROM ""AccountType""
                ORDER BY ""Title"";
                ";
            var qparams = new { };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryAsync<AccountTypeDto>(query, qparams, _unitOfWork.Transaction);
        }
    }
}