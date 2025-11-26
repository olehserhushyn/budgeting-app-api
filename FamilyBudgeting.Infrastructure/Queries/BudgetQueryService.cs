using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Budgets;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class BudgetQueryService : IBudgetQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BudgetQueryService> _logger;

        public BudgetQueryService(IUnitOfWork unitOfWork, ILogger<BudgetQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsFromLedgerAsync(Guid ledgerId)
        {
            string query = @"
                SELECT ""Id"",
                      ""LedgerId"",
                      ""StartDate"",
                      ""EndDate"",
                      ""CreatedAt"",
                      ""UpdatedAt"",
                      ""IsDeleted"",
                      ""Title""
                FROM ""Budget""
                WHERE ""LedgerId"" = @LedgerId
                AND ""IsDeleted"" = false
                ORDER BY ""StartDate"" DESC
                ";
            _logger.LogQuery(query, new { LedgerId = ledgerId });

            return await _unitOfWork.Connection.QueryAsync<BudgetDto>(query, new { LedgerId = ledgerId }, _unitOfWork.Transaction);
        }

        public async Task<BudgetDto?> GetBudgetAsync(Guid budgetId)
        {
            string query = @"
                SELECT ""Id"", ""LedgerId"", ""StartDate"", ""EndDate"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""Title""
                FROM ""Budget""
                WHERE ""Id"" = @BudgetId
                AND ""IsDeleted"" = false
                ";
            _logger.LogQuery(query, new { BudgetId = budgetId });

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<BudgetDto>(query, new { BudgetId = budgetId }, _unitOfWork.Transaction);
        }

        public async Task<Guid> GetLedgerIdFromBudgetAsync(Guid budgetId)
        {
            string query = @"
                SELECT ""LedgerId""
                FROM ""Budget""
                WHERE ""Id"" = @BudgetId
                AND ""IsDeleted"" = false
                ";
            _logger.LogQuery(query, new { BudgetId = budgetId });

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, new { BudgetId = budgetId }, _unitOfWork.Transaction);
        }
    }
}