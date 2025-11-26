using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Budgets;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BudgetRepository> _logger;

        public BudgetRepository(IUnitOfWork unitOfWork, ILogger<BudgetRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateBudgetAsync(Budget budget)
        {
            string query = @"
                INSERT INTO ""Budget""
                (""LedgerId"", ""StartDate"", ""EndDate"", ""Title"")
                VALUES
                (@LedgerId, @StartDate, @EndDate, @Title)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                LedgerId = budget.LedgerId,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Title = budget.Title
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateBudgetAsync(Guid id, Budget budget)
        {
            string query = @"
                UPDATE ""Budget""
                SET ""LedgerId"" = @LedgerId, 
                    ""StartDate"" = @StartDate, 
                    ""EndDate"" = @EndDate, 
                    ""Title"" = @Title, 
                    ""IsDeleted"" = @IsDeleted, 
                    ""UpdatedAt"" = @UpdatedAt
                WHERE ""Id"" = @Id;
                ";
            var qparams = new
            {
                Id = id,
                LedgerId = budget.LedgerId,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Title = budget.Title,
                IsDeleted = budget.IsDeleted,
                UpdatedAt = budget.UpdatedAt
            };
            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction) > 0;
        }
    }
}