using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class BudgetCategoryQueryService : IBudgetCategoryQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BudgetCategoryQueryService> _logger;

        public BudgetCategoryQueryService(IUnitOfWork unitOfWork, ILogger<BudgetCategoryQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<GetBudgetCategoriesDetailedResponse>> GetBudgetCategoriesAsync(Guid ledgerId, Guid budgetId)
        {
            string query = @"
                SELECT
                    bc.""Id"" AS Id, 
                    c_main.""Id"" AS CategoryId,
                    c_main.""Title"" AS CategoryName,
                    bc.""PlannedAmount"", 
                    bc.""InitialPlannedAmount"", 
                    bc.""CurrentAmount"", 
                    bc.""CurrencyId"", 
                    curr.""Code"" AS CurrencyCode, 
                    curr.""Name"" AS CurrencyName,
                    curr.""Symbol"" AS CurrencySymbol, 
                    curr.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor,
                    c_main.""TransactionTypeId"" as TransactionTypeId,
                    tt.""Title"" as TransactionTypeTitle
                FROM ""BudgetCategory"" AS bc
                INNER JOIN ""Category"" AS c_main ON bc.""CategoryId"" = c_main.""Id""
                INNER JOIN ""TransactionType"" as tt ON tt.""Id"" = c_main.""TransactionTypeId""
                JOIN ""Currency"" AS curr ON bc.""CurrencyId"" = curr.""Id""
                INNER JOIN ""Budget"" AS b ON bc.""BudgetId"" = b.""Id""
                WHERE b.""LedgerId"" = @LedgerId AND bc.""BudgetId"" = @BudgetId AND b.""IsDeleted"" = false AND bc.""IsDeleted"" = false
                ORDER BY c_main.""Title"" ASC;
            ";
            var qparams = new
            {
                LedgerId = ledgerId,
                BudgetId = budgetId
            };
            _logger.LogQuery(query, qparams);
            return await _unitOfWork.Connection.QueryAsync<GetBudgetCategoriesDetailedResponse>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<GetBudgetCategoriesDetailedResponse?> GetBudgetCategoryAsync(Guid ledgerId, Guid budgetId, Guid id)
        {
            string query = @"
                SELECT
                    bc.""Id"" AS Id,
                    c_main.""Id"" AS CategoryId,
                    c_main.""Title"" AS CategoryName,
                    bc.""PlannedAmount"", 
                    bc.""InitialPlannedAmount"", 
                    bc.""CurrentAmount"", 
                    bc.""CurrencyId"", 
                    curr.""Code"" AS CurrencyCode, 
                    curr.""Name"" AS CurrencyName,
                    curr.""Symbol"" AS CurrencySymbol, 
                    curr.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor,
                    c_main.""TransactionTypeId"" as TransactionTypeId,
                    tt.""Title"" as TransactionTypeTitle
                FROM ""BudgetCategory"" AS bc
                INNER JOIN ""Category"" AS c_main ON bc.""CategoryId"" = c_main.""Id""
                INNER JOIN ""TransactionType"" as tt ON tt.""Id"" = c_main.""TransactionTypeId""
                JOIN ""Currency"" AS curr ON bc.""CurrencyId"" = curr.""Id""
                INNER JOIN ""Budget"" AS b ON bc.""BudgetId"" = b.""Id""
                WHERE b.""LedgerId"" = @LedgerId AND bc.""BudgetId"" = @BudgetId AND b.""IsDeleted"" = false AND bc.""Id"" = @Id;
            ";
            var qparams = new
            {
                LedgerId = ledgerId,
                BudgetId = budgetId,
                Id = id
            };
            _logger.LogQuery(query, qparams);
            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<GetBudgetCategoriesDetailedResponse>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<BudgetCategoriesDetailedResponse>> GetBudgetCategoriesDetailedAsync(Guid ledgerId, Guid budgetId)
        {
            string query = @"
                SELECT
                    bc.""Id"" AS Id, 
                    c_main.""Id"" AS CategoryId,
                    c_main.""Title"" AS CategoryName,
                    bc.""PlannedAmount"", 
                    bc.""InitialPlannedAmount"", 
                    bc.""CurrentAmount"", 
                    COALESCE(SUM(t.""Amount""), 0) AS SpentAmount,
                    bc.""CurrencyId"", 
                    curr.""Code"" AS CurrencyCode, 
                    curr.""Name"" AS CurrencyName,
                    curr.""Symbol"" AS CurrencySymbol, 
                    curr.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor,
                    c_main.""TransactionTypeId"" as TransactionTypeId,
                    tt.""Title"" as TransactionTypeTitle
                FROM ""BudgetCategory"" AS bc
                INNER JOIN ""Category"" AS c_main ON bc.""CategoryId"" = c_main.""Id""
                INNER JOIN ""TransactionType"" as tt ON tt.""Id"" = c_main.""TransactionTypeId""
                JOIN ""Currency"" AS curr ON bc.""CurrencyId"" = curr.""Id""
                INNER JOIN ""Budget"" AS b ON bc.""BudgetId"" = b.""Id""
                LEFT JOIN ""Transaction"" AS t ON bc.""Id"" = t.""BudgetCategoryId"" 
                    AND t.""BudgetId"" = @BudgetId 
                    AND t.""LedgerId"" = @LedgerId 
                    -- AND t.""Date""::date BETWEEN b.""StartDate""::date AND b.""EndDate""::date -- IMPORTANT
                WHERE b.""LedgerId"" = @LedgerId AND bc.""BudgetId"" = @BudgetId AND b.""IsDeleted"" = false AND bc.""IsDeleted"" = false
                GROUP BY 
                    bc.""Id"",
                    c_main.""Id"",
                    c_main.""Title"",
                    bc.""PlannedAmount"",
                    bc.""CurrentAmount"",
                    bc.""CurrencyId"",
                    curr.""Code"",
                    curr.""Name"",
                    curr.""Symbol"",
                    curr.""FractionalUnitFactor"",
                    c_main.""TransactionTypeId"",
                    tt.""Title""
                ORDER BY c_main.""Title"";
            ";

            var qparams = new
            {
                LedgerId = ledgerId,
                BudgetId = budgetId
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryAsync<BudgetCategoriesDetailedResponse>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<BudgetCategoryDto?> GetBudgetCategoryAsync(Guid id)
        {
            string query = @"
                SELECT bc.""Id"",
                      bc.""BudgetId"",
                      bc.""CategoryId"",
                      bc.""PlannedAmount"",
                      bc.""InitialPlannedAmount"",
                      bc.""CreatedAt"",
                      bc.""UpdatedAt"",
                      bc.""IsDeleted"",
                      bc.""CurrencyId"",
                      bc.""CurrentAmount"",
                      curr.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor
                FROM ""BudgetCategory"" as bc
                INNER JOIN ""Currency"" AS curr ON bc.""CurrencyId"" = curr.""Id""
                WHERE bc.""IsDeleted"" = false AND bc.""Id"" = @Id;
            ";
            var qparams = new
            {
                Id = id
            };
            _logger.LogQuery(query, qparams);
            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<BudgetCategoryDto>(query, qparams, _unitOfWork.Transaction);
        }
    }
}