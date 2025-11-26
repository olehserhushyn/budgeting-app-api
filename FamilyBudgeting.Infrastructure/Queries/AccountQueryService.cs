using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Accounts;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class AccountQueryService : IAccountQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountQueryService> _logger;

        public AccountQueryService(IUnitOfWork unitOfWork, ILogger<AccountQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<Domain.DTOs.Models.Accounts.AccountCurrencyDetailsDto>> GetAccountsAsync(Guid userId)
        {
            string query = @"
                SELECT a.""Id"" as AccountId, a.""UserId"", a.""AccountTypeId"", a.""Title"" as AccountTitle, 
                    a.""Balance"" as AccountBalance, a.""CreatedAt"" as AccountCreatedAt, at.""Title"" as AccountTypeName,
                    c.""Id"" as CurrencyId, c.""Code"" as CurrencyCode, c.""Name"" as CurrencyName, c.""Symbol"" as CurrencySymbol, 
                    c.""FractionalUnitFactor"" as CurrencyFractionalUnitFactor
                FROM ""Account"" as a
                INNER JOIN ""AccountType"" as at ON a.""AccountTypeId"" = at.""Id""  
                INNER JOIN ""Currency"" as c ON a.""CurrencyId"" = c.""Id"" 
                WHERE a.""UserId"" = @UserId AND a.""IsDeleted"" = false;
                ";
            var qparams = new
            {
                UserId = userId
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection
                .QueryAsync<Domain.DTOs.Models.Accounts.AccountCurrencyDetailsDto>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<Domain.DTOs.Models.Accounts.AccountCurrencyDetailsDto?> GetAccountAsync(Guid accountId)
        {
            string query = @"
                SELECT a.""Id"" as AccountId, a.""UserId"", a.""AccountTypeId"", a.""Title"" as AccountTitle, 
                    a.""Balance"" as AccountBalance, a.""CreatedAt"" as AccountCreatedAt, at.""Title"" as AccountTypeName,
                    c.""Id"" as CurrencyId, c.""Code"" as CurrencyCode, c.""Name"" as CurrencyName, c.""Symbol"" as CurrencySymbol, 
                    c.""FractionalUnitFactor"" as CurrencyFractionalUnitFactor
                FROM ""Account"" as a
                INNER JOIN ""AccountType"" as at ON a.""AccountTypeId"" = at.""Id""  
                INNER JOIN ""Currency"" as c ON a.""CurrencyId"" = c.""Id""  
                WHERE a.""Id"" = @AccountId AND a.""IsDeleted"" = false;
                ";
            var qparams = new
            {
                AccountId = accountId
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection
                .QueryFirstOrDefaultAsync<Domain.DTOs.Models.Accounts.AccountCurrencyDetailsDto>(query, qparams, _unitOfWork.Transaction);
        }

        public IQueryBuilder<AccountCurrencyDetailsDto?> GetAccountCurrencyDetailsAsync(Guid accountId)
        {
            string query = @"
                SELECT a.""Id"" as AccountId, a.""UserId"", a.""AccountTypeId"", a.""Title"" as AccountTitle, 
                    a.""Balance"" as AccountBalance, a.""CreatedAt"" as AccountCreatedAt, at.""Title"" as AccountTypeName,
                    c.""Id"" as CurrencyId, c.""Code"" as CurrencyCode, c.""Name"" as CurrencyName, c.""Symbol"" as CurrencySymbol, 
                    c.""FractionalUnitFactor"" as CurrencyFractionalUnitFactor
                FROM ""Account"" as a
                INNER JOIN ""AccountType"" as at ON a.""AccountTypeId"" = at.""Id""  
                INNER JOIN ""Currency"" as c ON a.""CurrencyId"" = c.""Id""
                WHERE a.""Id"" = @AccountId AND a.""IsDeleted"" = false;
                ";
            var qparams = new
            {
                AccountId = accountId
            };
            _logger.LogQuery(query, qparams);

            return _unitOfWork.CreateQueryBuilder<AccountCurrencyDetailsDto>(query, qparams, _logger);
        }

        public async Task<AccountDto?> GetAccountDtoAsync(Guid accountId)
        {
            string query = @"
                SELECT 
                    ""Id"", 
                    ""CreatedAt"", 
                    ""UpdatedAt"", 
                    ""IsDeleted"", 
                    ""UserId"", 
                    ""AccountTypeId"", 
                    ""Title"", 
                    ""Balance"",
                    ""CurrencyId""
                FROM ""Account""
                WHERE ""Id"" = @AccountId AND ""IsDeleted"" = false;
                ";
            var qparams = new
            {
                AccountId = accountId
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<AccountDto>(query, qparams, _unitOfWork.Transaction);
        }
    }
}