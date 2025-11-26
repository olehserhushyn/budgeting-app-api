using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class BudgetCategoryRepository : IBudgetCategoryRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BudgetCategoryRepository> _logger;

        public BudgetCategoryRepository(IUnitOfWork unitOfWork, ILogger<BudgetCategoryRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateBudgetCategoryAsync(BudgetCategory budgetCategory)
        {
            string query = @"
                INSERT INTO ""BudgetCategory""
                (""BudgetId"", ""CategoryId"", ""CurrencyId"", ""PlannedAmount"", ""CurrentAmount"")
                VALUES
                (@BudgetId, @CategoryId, @CurrencyId, @PlannedAmount, @CurrentAmount)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                BudgetId = budgetCategory.BudgetId,
                CategoryId = budgetCategory.CategoryId,
                CurrencyId = budgetCategory.CurrencyId,
                PlannedAmount = budgetCategory.PlannedAmount,
                CurrentAmount = budgetCategory.CurrentAmount
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateBudgetCategoryAsync(Guid id, BudgetCategory budgetCategory)
        {
            string query = @"
                UPDATE ""BudgetCategory""
                SET 
                    ""BudgetId"" = @BudgetId,
                    ""CategoryId"" = @CategoryId,
                    ""CurrencyId"" = @CurrencyId,
                    ""PlannedAmount"" = @PlannedAmount,
                    ""CurrentAmount"" = @CurrentAmount,
                    ""UpdatedAt"" = @UpdatedAt,
                    ""IsDeleted"" = @IsDeleted
                WHERE ""Id"" = @Id;
            ";

            var qparams = new
            {
                Id = id,
                BudgetId = budgetCategory.BudgetId,
                CategoryId = budgetCategory.CategoryId,
                CurrencyId = budgetCategory.CurrencyId,
                PlannedAmount = budgetCategory.PlannedAmount,
                CurrentAmount = budgetCategory.CurrentAmount,
                UpdatedAt = budgetCategory.UpdatedAt,
                IsDeleted = budgetCategory.IsDeleted
            };

            _logger.LogQuery(query, qparams);

            int affectedRows = await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction);
            return affectedRows > 0;
        }
    }
}