using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionPageDataQueryHandler : ITransactionPageDataQueryHandler
    {
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IBudgetQueryService _budgetQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly ICategoryQueryService _categoryQueryService;

        public TransactionPageDataQueryHandler(
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ILedgerQueryService ledgerQueryService,
            IBudgetQueryService budgetQueryService,
            IBudgetCategoryQueryService budgetCategoryQueryService,
            ICategoryQueryService categoryQueryService)
        {
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _ledgerQueryService = ledgerQueryService;
            _budgetQueryService = budgetQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _categoryQueryService = categoryQueryService;
        }

        public async Task<Result<GetCreateTransactionPageDataResponse>> HandleAsync(Guid userId, Guid? budgetId, Guid? ledgerId)
        {
            var accounts = await _accountQueryService.GetAccountsAsync(userId);
            var transactionTypes = await _transactionTypeQueryService.GetTransactionsTypesAsync();

            Guid existingLedgerId;
            if (ledgerId is null)
            {
                var firstLedger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId);
                if (firstLedger is null)
                {
                    return Result.NotFound("No ledgers found for the user");
                }
                existingLedgerId = firstLedger.Id;
            }
            else
            {
                existingLedgerId = ledgerId.Value;
            }

            var budgets = await _budgetQueryService.GetBudgetsFromLedgerAsync(existingLedgerId);

            Dictionary<Guid, CategoryWithTypeDto> budgetCategories = new();
            Dictionary<Guid, CategoryWithTypeDto> transactionCategories = new();

            if (budgetId is not null)
            {
                var categories = await _budgetCategoryQueryService.GetBudgetCategoriesAsync(existingLedgerId, budgetId.Value);
                budgetCategories = categories.ToDictionary(x => x.Id, x => new CategoryWithTypeDto
                {
                    Title = x.CategoryName,
                    TransactionTypeId = x.TransactionTypeId,
                    TransactionTypeTitle = x.TransactionTypeTitle
                });
            }
            else
            {
                var categories = await _categoryQueryService.GetCategoriesAsync(existingLedgerId);
                transactionCategories = categories.ToDictionary(x => x.Id, x => new CategoryWithTypeDto
                {
                    Title = x.Title,
                    TransactionTypeId = x.TransactionTypeId,
                    TransactionTypeTitle = string.Empty
                });
            }

            return Result.Success(new GetCreateTransactionPageDataResponse
            {
                Accounts = accounts?.ToDictionary(x => x.AccountId, x => string.Join('|', x.AccountTitle, x.CurrencySymbol)),
                TransactionCategories = transactionCategories,
                BudgetCategories = budgetCategories,
                TransactionTypes = transactionTypes?.ToDictionary(x => x.Id, x => x.Title),
                Budgets = budgets?.ToDictionary(x => x.Id, x => $"{x.StartDate:yyyy-MM-dd}-{x.EndDate:yyyy-MM-dd}")
            });
        }
    }
}
