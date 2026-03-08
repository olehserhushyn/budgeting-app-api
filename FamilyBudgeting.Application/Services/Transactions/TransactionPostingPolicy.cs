using Ardalis.Result;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Models.Accounts;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionPostingPolicy : ITransactionPostingPolicy
    {
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly IAccountRepository _accountRepository;

        public TransactionPostingPolicy(
            IBudgetCategoryQueryService budgetCategoryQueryService,
            IBudgetCategoryRepository budgetCategoryRepository,
            IAccountRepository accountRepository)
        {
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _budgetCategoryRepository = budgetCategoryRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result> ApplyBudgetImpactForCreateAsync(Guid ledgerId, CreateTransactionRequest request, int centsAmountWithSign)
        {
            if (request.BudgetId is null || request.BudgetCategoryId is null)
            {
                return Result.Success();
            }

            var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(
                ledgerId,
                request.BudgetId.Value,
                request.BudgetCategoryId.Value);

            if (budgetCategoryDto is null)
            {
                return Result.NotFound("Budget Category not found");
            }

            var budgetCategory = new BudgetCategory(
                request.BudgetId.Value,
                budgetCategoryDto.CategoryId,
                budgetCategoryDto.CurrencyId,
                budgetCategoryDto.PlannedAmount,
                budgetCategoryDto.CurrentAmount,
                budgetCategoryDto.InitialPlannedAmount);

            budgetCategory.AddTransaction(centsAmountWithSign);

            var budgetCategoryResult = await _budgetCategoryRepository.UpdateBudgetCategoryAsync(budgetCategoryDto.Id, budgetCategory);
            if (!budgetCategoryResult)
            {
                return Result.Error("Unexpected error during updating budget category");
            }

            return Result.Success();
        }

        public async Task<Result> ApplyAccountImpactForCreateAsync(AccountCurrencyDetailsDto accountDto, int centsAmountWithSign)
        {
            var account = new Account(
                accountDto.UserId,
                accountDto.AccountTypeId,
                accountDto.AccountTitle,
                accountDto.AccountBalance,
                accountDto.CurrencyId);

            account.AddTransaction(centsAmountWithSign);

            var accountResult = await _accountRepository.UpdateAccountAsync(accountDto.AccountId, account);
            if (!accountResult)
            {
                return Result.Error("Unexpected error during updating account");
            }

            return Result.Success();
        }
    }
}
