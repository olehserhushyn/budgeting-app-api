using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Ledgers;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class LedgerQueryService : ILedgerQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LedgerQueryService> _logger;

        public LedgerQueryService(IUnitOfWork unitOfWork, ILogger<LedgerQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<LedgerDto>> GetUserLedgersAsync(Guid userId)
        {
            string query = @"
                SELECT 
                    l.""Id"" as Id, 
                    l.""Title"" as Title,
                    l.""CreatedAt"" as CreatedAt,
                    l.""UpdatedAt"" as UpdatedAt,
                    l.""IsDeleted"" as IsDeleted
                FROM ""UserLedger"" as ul
                INNER JOIN ""Ledger"" l ON ul.""LedgerId"" = l.""Id""
                WHERE ul.""UserId"" = @UserId 
                AND l.""IsDeleted"" = false AND ul.""IsDeleted"" = false
                ORDER BY ul.""CreatedAt"" ASC
                ";
            _logger.LogQuery(query, new { UserId = userId });

            return await _unitOfWork.Connection.QueryAsync<LedgerDto>(query, new { UserId = userId }, _unitOfWork.Transaction);
        }

        public async Task<LedgerDto?> GetUserLedgerFirstAsync(Guid userId)
        {
            string query = @"
                SELECT
                    l.""Id"" as Id, 
                    l.""Title"" as Title,
                    l.""CreatedAt"" as CreatedAt,
                    l.""UpdatedAt"" as UpdatedAt,
                    l.""IsDeleted"" as IsDeleted
                FROM ""UserLedger"" as ul
                INNER JOIN ""Ledger"" l ON ul.""LedgerId"" = l.""Id""
                WHERE ul.""UserId"" = @UserId 
                AND l.""IsDeleted"" = false AND ul.""IsDeleted"" = false
                ORDER BY ul.""CreatedAt"" ASC
                LIMIT 1
                ";
            _logger.LogQuery(query, new { UserId = userId });

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<LedgerDto>(query, new { UserId = userId }, _unitOfWork.Transaction);
        }

        public async Task<LedgerDto?> GetUserLedgerFirstAsync(Guid userId, Guid budgetId)
        {
            string query = @"
                SELECT
                    l.""Id"" as Id, 
                    l.""Title"" as Title,
                    l.""CreatedAt"" as CreatedAt,
                    l.""UpdatedAt"" as UpdatedAt,
                    l.""IsDeleted"" as IsDeleted
                FROM ""UserLedger"" as ul
                INNER JOIN ""Ledger"" l ON ul.""LedgerId"" = l.""Id""
                INNER JOIN ""Budget"" b ON b.""LedgerId"" = l.""Id""
                WHERE ul.""UserId"" = @UserId 
                AND b.""Id"" = @BudgetId
                AND l.""IsDeleted"" = false AND ul.""IsDeleted"" = false
                ORDER BY ul.""CreatedAt"" ASC
                LIMIT 1
                ";

            var qparams = new
            {
                UserId = userId,
                BudgetId = budgetId
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<LedgerDto>(query, qparams, _unitOfWork.Transaction);
        }
    }
}