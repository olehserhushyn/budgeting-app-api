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

            return await ApplyBudgetCategoryTransactionAsync(
                ledgerId,
                request.BudgetId.Value,
                request.BudgetCategoryId.Value,
                centsAmountWithSign);
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

            return await UpdateAccountAsync(accountDto.AccountId, account);
        }

        public async Task<Result> ApplyBudgetImpactForUpdateAsync(Guid ledgerId, UpdateTransactionRequest request, int newCentsAmountWithSign)
        {
            if (request.BudgetId is null || request.BudgetCategoryId is null)
            {
                return Result.Success();
            }

            return await ApplyBudgetCategoryTransactionAsync(
                ledgerId,
                request.BudgetId.Value,
                request.BudgetCategoryId.Value,
                newCentsAmountWithSign);
        }

        public async Task<Result> ApplyAccountImpactForUpdateAsync(AccountCurrencyDetailsDto accountDto, int existingCentsAmountWithSign, int newCentsAmountWithSign)
        {
            var account = new Account(
                accountDto.UserId,
                accountDto.AccountTypeId,
                accountDto.AccountTitle,
                accountDto.AccountBalance,
                accountDto.CurrencyId);

            account.UpdateTransaction(existingCentsAmountWithSign, newCentsAmountWithSign);

            return await UpdateAccountAsync(accountDto.AccountId, account);
        }

        public async Task<Result> ApplyAccountImpactForDeleteAsync(AccountCurrencyDetailsDto accountDto, int existingCentsAmountWithSign)
        {
            var account = new Account(
                accountDto.UserId,
                accountDto.AccountTypeId,
                accountDto.AccountTitle,
                accountDto.AccountBalance,
                accountDto.CurrencyId);

            account.RemoveTransaction(existingCentsAmountWithSign);

            return await UpdateAccountAsync(accountDto.AccountId, account);
        }

        private async Task<Result> ApplyBudgetCategoryTransactionAsync(Guid ledgerId, Guid budgetId, Guid budgetCategoryId, int centsAmountWithSign)
        {
            var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(ledgerId, budgetId, budgetCategoryId);
            if (budgetCategoryDto is null)
            {
                return Result.NotFound("Budget Category not found");
            }

            var budgetCategory = new BudgetCategory(
                budgetId,
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

        private async Task<Result> UpdateAccountAsync(Guid accountId, Account account)
        {
            var accountResult = await _accountRepository.UpdateAccountAsync(accountId, account);
            if (!accountResult)
            {
                return Result.Error("Unexpected error during updating account");
            }

            return Result.Success();
        }
    }
}
