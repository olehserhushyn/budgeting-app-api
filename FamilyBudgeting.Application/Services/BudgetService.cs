using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Budgets;
using FamilyBudgeting.Domain.DTOs.Requests.Budgets;
using FamilyBudgeting.Domain.DTOs.Responses.Budgets;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.Budgets;
using FamilyBudgeting.Domain.Data.ValueObjects;

namespace FamilyBudgeting.Domain.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IBudgetQueryService _budgetQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;

        public BudgetService(IBudgetRepository budgetRepository, IBudgetQueryService budgetQueryService, 
            IBudgetCategoryQueryService budgetCategoryQueryService)
        {
            _budgetRepository = budgetRepository;
            _budgetQueryService = budgetQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
        }

        public async Task<Result<Guid>> CreateBudgetAsync(CreateBudgetRequest request)
        {
            var budget = new Budget(request.LedgerId, request.StartDate, request.EndDate, request.Title);

            Guid bId = await _budgetRepository.CreateBudgetAsync(budget);

            return Result.Success(bId);
        }

        public async Task<Result<IEnumerable<GetLedgerBudgetsResponse>>> GetBudgetsFromLedgerAsync(Guid ledgerId)
        {
            var budgets = await _budgetQueryService.GetBudgetsFromLedgerAsync(ledgerId);
            return Result<IEnumerable<GetLedgerBudgetsResponse>>.Success(budgets.Select(x => new GetLedgerBudgetsResponse
            {
                BudgetTitle = x.Title,
                EndDate = x.EndDate,
                StartDate = x.StartDate,
                Id = x.Id,
            }));
        }

        public async Task<Result<Guid>> GetLedgerIdFromBudgetAsync(Guid budgetId)
        {
            var ledgerId = await _budgetQueryService.GetLedgerIdFromBudgetAsync(budgetId);

            if (ledgerId == Guid.Empty)
            {
                return Result<Guid>.Error("Unable to find Ledger");
            }

            return Result<Guid>.Success(ledgerId);
        }

        public async Task<Result<BudgetDto>> GetBudgetAsync(Guid budgetId)
        {
            var budget = await _budgetQueryService.GetBudgetAsync(budgetId);
            if (budget is null)
            {
                return Result<BudgetDto>.NotFound("Budget not found");
            }
            return Result<BudgetDto>.Success(budget);
        }

        public async Task<Result<GetBudgetDetailsResponse>> GetBudetDetailsAsync(Guid budgetId)
        {
            var budget = await _budgetQueryService.GetBudgetAsync(budgetId);
            if (budget is null)
            {
                return Result<GetBudgetDetailsResponse>.NotFound("Budget not found");
            }

            var categories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(budget.LedgerId, budget.Id);
            int factor = categories.FirstOrDefault()?.CurrencyFractionalUnitFactor ?? 100;

            GetBudgetDetailsResponse budgetDetails = new GetBudgetDetailsResponse()
            {
                BudgetId = budgetId,
                BudgetTitle = budget.Title,
                BudgetStartDate = budget.StartDate,
                BudgetEndDate = budget.EndDate,
                BudgetCategories = categories?.ToList() ?? new(),
                ExpenseTotalPlannedAmount = categories?.Where(c => c.TransactionTypeTitle == CategoryTypes.Expense).Sum(c => c.PlannedAmount) ?? 0,
                ExpenseTotalSpentAmount = categories?.Where(c => c.TransactionTypeTitle == CategoryTypes.Expense).Sum(c => c.SpentAmount) ?? 0,
                IncomeTotalPlannedAmount = categories?.Where(c => c.TransactionTypeTitle == CategoryTypes.Income).Sum(c => c.PlannedAmount) ?? 0,
                IncomeTotalSpentAmount = categories?.Where(c => c.TransactionTypeTitle == CategoryTypes.Income).Sum(c => c.SpentAmount) ?? 0,
                CurrencyFractionalUnitFactor = factor,
            };

            return Result<GetBudgetDetailsResponse>.Success(budgetDetails);
        }

        public async Task<Result<Guid>> UpdateBudgetAsync(Guid budgetId, UpdateBudgetRequest request)
        {
            var budgetDto = await _budgetQueryService.GetBudgetAsync(budgetId);
            if (budgetDto is null)
            {
                return Result<Guid>.NotFound("Budget not found");
            }
            var budget = new Budget(budgetDto.LedgerId, budgetDto.StartDate, budgetDto.EndDate, budgetDto.Title)
            {
                Id = budgetDto.Id
            };
            budget.UpdateDetails(request.LedgerId, request.StartDate, request.EndDate, request.Title);
            var updated = await _budgetRepository.UpdateBudgetAsync(budgetId, budget);
            if (!updated)
            {
                return Result<Guid>.Error("Failed to update budget");
            }
            return Result<Guid>.Success(budgetId);
        }

        public async Task<Result> DeleteBudgetAsync(Guid budgetId)
        {
            var budgetDto = await _budgetQueryService.GetBudgetAsync(budgetId);
            if (budgetDto is null)
            {
                return Result.NotFound("Budget not found");
            }
            var budget = new Budget(budgetDto.LedgerId, budgetDto.StartDate, budgetDto.EndDate, budgetDto.Title);
            budget.Delete();

            var deleted = await _budgetRepository.UpdateBudgetAsync(budgetId, budget);
            if (!deleted)
            {
                return Result.Error("Failed to delete budget");
            }
            return Result.Success();
        }
    }
}
