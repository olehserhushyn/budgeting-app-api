using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class AccessQueryService : IAccessQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccessQueryService> _logger;

        public AccessQueryService(IUnitOfWork unitOfWork, ILogger<AccessQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> UserHasAccessToLedgerAsync(Guid userId, Guid ledgerId)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM ""UserLedger"" 
                WHERE ""UserId"" = @UserId AND ""LedgerId"" = @LedgerId AND ""IsDeleted"" = false
            ";

            var qparams = new
            {
                UserId = userId,
                LedgerId = ledgerId
            };

            _logger.LogQuery(query, qparams);
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<bool> UserHasAccessToAccountAsync(Guid userId, Guid accountId)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM ""Account"" 
                WHERE ""UserId"" = @UserId AND ""Id"" = @AccountId AND ""IsDeleted"" = false
            ";
            var qparams = new
            {
                UserId = userId,
                AccountId = accountId
            };
            _logger.LogQuery(query, qparams);
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<bool> UserHasAccessToTransactionAsync(Guid userId, Guid transactionId)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM ""Transaction"" as t
                INNER JOIN ""Ledger"" as l ON t.""LedgerId"" = l.""Id""
                WHERE l.""UserId"" = @UserId AND t.""Id"" = @TransactionId AND l.""IsDeleted"" = false AND t.""IsDeleted"" = false
            ";
            var qparams = new
            {
                UserId = userId,
                TransactionId = transactionId
            };
            _logger.LogQuery(query, qparams);
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<bool> UserHasAccessToBudgetAsync(Guid userId, Guid budgetId)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM ""Budget"" as b
                INNER JOIN ""Ledger"" as l ON b.""LedgerId"" = l.""Id""
                INNER JOIN ""UserLedger"" as ul ON l.""Id"" = ul.""LedgerId""
                WHERE ul.""UserId"" = @UserId AND b.""Id"" = @BudgetId AND l.""IsDeleted"" = false AND b.""IsDeleted"" = false
            ";
            var qparams = new
            {
                UserId = userId,
                BudgetId = budgetId
            };
            _logger.LogQuery(query, qparams);
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<bool> UserHasAccessToBudgetCategoryAsync(Guid userId, Guid bCategoryId)
        {
            string query = @"
                SELECT COUNT(1) 
                FROM ""BudgetCategory"" as bc
                INNER JOIN ""Budget"" as b ON bc.""BudgetId"" = b.""Id""
                INNER JOIN ""UserLedger"" as ul ON b.""LedgerId"" = ul.""LedgerId""
                WHERE ul.""UserId"" = @UserId AND bc.""Id"" = @BudgetCategoryId AND b.""IsDeleted"" = false AND bc.""IsDeleted"" = false
            ";
            var qparams = new
            {
                UserId = userId,
                BudgetCategoryId = bCategoryId
            };
            _logger.LogQuery(query, qparams);
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<bool> UserHasLedgerRolesAsync(Guid userId, Guid ledgerId, params string[] ledgerRoleTitle)
        {
            string query = @"
                SELECT COUNT(*)
                FROM ""UserLedger"" ul
                JOIN ""UserLedgerRole"" ulr ON ul.""RoleId"" = ulr.""Id""
                WHERE ul.""UserId"" = @UserId
                  AND ul.""LedgerId"" = @LedgerId
                  AND ulr.""Title"" = ANY(@RoleTitle);
            ";
            var qparams = new
            {
                UserId = userId,
                LedgerId = ledgerId,
                RoleTitle = ledgerRoleTitle,
            };
            _logger.LogQuery(query, qparams);
            var count = await _unitOfWork.Connection.ExecuteScalarAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }
    }
}