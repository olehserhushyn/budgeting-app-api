using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountRepository> _logger;

        public AccountRepository(IUnitOfWork unitOfWork, ILogger<AccountRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateAccountAsync(Account account)
        {
            string query = @"
                INSERT INTO ""Account""
                (""UserId"", ""AccountTypeId"", ""Title"", ""Balance"", ""CurrencyId"")
                VALUES
                (@UserId, @AccountTypeId, @Title, @Balance, @CurrencyId)
                RETURNING ""Id"";
                ";
            var qparams = new
            {
                UserId = account.UserId,
                AccountTypeId = account.AccountTypeId,
                Title = account.Title,
                Balance = account.Balance,
                CurrencyId = account.CurrencyId
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateAccountAsync(Guid Id, Account account)
        {
            string query = @"
                UPDATE ""Account""
                SET ""AccountTypeId"" = @AccountTypeId, 
                    ""Title"" = @Title, 
                    ""Balance"" = @Balance, 
                    ""UpdatedAt"" = @UpdatedAt, 
                    ""CurrencyId"" = @CurrencyId,
                    ""IsDeleted"" = @IsDeleted
                WHERE ""Id"" = @Id;
                ";
            var qparams = new
            {
                Id = Id,
                AccountTypeId = account.AccountTypeId,
                Title = account.Title,
                Balance = account.Balance,
                UpdatedAt = account.UpdatedAt,
                CurrencyId = account.CurrencyId,
                IsDeleted = account.IsDeleted
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction) > 0;
        }
    }
}