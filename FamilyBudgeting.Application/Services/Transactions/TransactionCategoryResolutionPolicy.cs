using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionCategoryResolutionPolicy : ITransactionCategoryResolutionPolicy
    {
        private readonly ICategoryQueryService _categoryQueryService;
        private readonly ICurrencyQueryService _currencyQueryService;
        private readonly IBudgetQueryService _budgetQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ICategoryService _categoryService;
        private readonly IBudgetCategoryService _budgetCategoryService;

        public TransactionCategoryResolutionPolicy(
            ICategoryQueryService categoryQueryService,
            ICurrencyQueryService currencyQueryService,
            IBudgetQueryService budgetQueryService,
            IBudgetCategoryQueryService budgetCategoryQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ICategoryService categoryService,
            IBudgetCategoryService budgetCategoryService)
        {
            _categoryQueryService = categoryQueryService;
            _currencyQueryService = currencyQueryService;
            _budgetQueryService = budgetQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _categoryService = categoryService;
            _budgetCategoryService = budgetCategoryService;
        }

        public async Task<Result<(Guid? CategoryId, CreateTransactionRequest Request)>> ResolveForCreateAsync(
            Guid userId,
            Guid ledgerId,
            CreateTransactionRequest request)
        {
            Guid? categoryId = request.CategoryId;

            if (categoryId == null && !string.IsNullOrWhiteSpace(request.BudgetCategoryTitle) && request.BudgetId is not null)
            {
                var existingBudgetCategories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(ledgerId, request.BudgetId.Value);
                var matched = existingBudgetCategories.FirstOrDefault(bc => bc.CategoryName.Equals(request.BudgetCategoryTitle, StringComparison.OrdinalIgnoreCase));
                if (matched is not null)
                {
                    categoryId = matched.CategoryId;
                    request = request with { BudgetCategoryId = matched.Id };
                }
                else
                {
                    var budget = await _budgetQueryService.GetBudgetAsync(request.BudgetId.Value);
                    if (budget is null)
                    {
                        return Result.NotFound("Budget not found");
                    }

                    var currencies = await _currencyQueryService.GetCurrenciesFromLedgerAsync();
                    var currency = currencies.FirstOrDefault();
                    if (currency is null)
                    {
                        return Result.NotFound("Currency not found for creating budget category");
                    }

                    if (await _transactionTypeQueryService.GetTransactionTypeAsync(request.TransactionTypeId) is null)
                    {
                        return Result.NotFound("Transaction type not found");
                    }

                    var existingCategory = await _categoryQueryService.GetCategoryAsync(request.BudgetCategoryTitle);
                    if (existingCategory is null)
                    {
                        var createLedgerCategoryReq = new CreateLedgerCategoryRequest(ledgerId, request.BudgetCategoryTitle, request.TransactionTypeId);
                        var createCatRes = await _categoryService.CreateLedgerCategoryAsync(createLedgerCategoryReq);
                        if (createCatRes.Status != ResultStatus.Ok)
                        {
                            return Result.Error("Failed to create base category for budget category");
                        }
                    }

                    var createBudgetCategoryReq = new CreateBudgetCategoryRequest(request.BudgetCategoryTitle, request.BudgetId.Value, currency.Id, 0, request.TransactionTypeId);
                    var createBudgetCatRes = await _budgetCategoryService.CreateBudgetCategoryAsync(userId, createBudgetCategoryReq);
                    if (createBudgetCatRes.Status != ResultStatus.Ok)
                    {
                        return Result.Error("Failed to create budget category");
                    }

                    var createdBudgetCategories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(ledgerId, request.BudgetId.Value);
                    var created = createdBudgetCategories.FirstOrDefault(bc => bc.CategoryName.Equals(request.BudgetCategoryTitle, StringComparison.OrdinalIgnoreCase));
                    if (created is null)
                    {
                        return Result.Error("Failed to retrieve created budget category");
                    }

                    categoryId = created.CategoryId;
                    request = request with { BudgetCategoryId = created.Id };
                }
            }

            if (categoryId == null && !string.IsNullOrWhiteSpace(request.CategoryTitle))
            {
                var existingCategory = await _categoryQueryService.GetCategoryAsync(request.CategoryTitle);
                if (existingCategory is not null)
                {
                    categoryId = existingCategory.Id;
                }
                else
                {
                    var createLedgerCategoryReq = new CreateLedgerCategoryRequest(ledgerId, request.CategoryTitle, request.TransactionTypeId);
                    var createCatRes = await _categoryService.CreateLedgerCategoryAsync(createLedgerCategoryReq);
                    if (createCatRes.Status != ResultStatus.Ok)
                    {
                        return Result.Error("Failed to create ledger category");
                    }
                    categoryId = createCatRes.Value;
                }
            }

            if (categoryId == null && request.BudgetId is not null && request.BudgetCategoryId is not null)
            {
                var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(ledgerId, request.BudgetId.Value, request.BudgetCategoryId.Value);
                if (budgetCategoryDto is null)
                {
                    return Result.NotFound("Budget Category not found");
                }
                categoryId = budgetCategoryDto.CategoryId;
            }

            return Result.Success((categoryId, request));
        }
    }
}
