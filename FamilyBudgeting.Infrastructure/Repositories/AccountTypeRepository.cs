using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.AccountTypes;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class AccountTypeRepository : IAccountTypeRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountTypeRepository> _logger;

        public AccountTypeRepository(IUnitOfWork unitOfWork, ILogger<AccountTypeRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateAccountTypeAsync(AccountType accountType)
        {
            string query = @"
                INSERT INTO ""AccountType""
                (""Title"")
                VALUES
                (@Title)
                RETURNING ""Id"";
                ";
            var qparams = new
            {
                Title = accountType.Title
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }
    }
}