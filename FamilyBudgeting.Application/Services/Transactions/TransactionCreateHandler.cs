using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionCreateHandler : TransactionCommandHandlerBase, ITransactionCreateHandler
    {
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ICategoryQueryService _categoryQueryService;
        private readonly ICurrencyQueryService _currencyQueryService;
        private readonly IBudgetQueryService _budgetQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly IAccountRepository _accountRepository;
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryService _categoryService;
        private readonly IBudgetCategoryService _budgetCategoryService;

        public TransactionCreateHandler(
            IUserLedgerQueryService userLedgerQueryService,
            ILedgerQueryService ledgerQueryService,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ICategoryQueryService categoryQueryService,
            ICurrencyQueryService currencyQueryService,
            IBudgetQueryService budgetQueryService,
            IBudgetCategoryQueryService budgetCategoryQueryService,
            IUnitOfWork unitOfWork,
            Microsoft.Extensions.Logging.ILogger<TransactionCreateHandler> logger,
            IAccountRepository accountRepository,
            IBudgetCategoryRepository budgetCategoryRepository,
            ITransactionRepository transactionRepository,
            ICategoryService categoryService,
            IBudgetCategoryService budgetCategoryService)
        : base(unitOfWork, logger)
        {
            _userLedgerQueryService = userLedgerQueryService;
            _ledgerQueryService = ledgerQueryService;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _categoryQueryService = categoryQueryService;
            _currencyQueryService = currencyQueryService;
            _budgetQueryService = budgetQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _accountRepository = accountRepository;
            _budgetCategoryRepository = budgetCategoryRepository;
            _transactionRepository = transactionRepository;
            _categoryService = categoryService;
            _budgetCategoryService = budgetCategoryService;
        }

        public async Task<Result<Guid>> HandleAsync(Guid userId, CreateTransactionRequest request)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var ledgerResult = await ResolveLedgerForCreateAsync(userId, request.LedgerId);
                if (!ledgerResult.IsSuccess)
                {
                    return Result.NotFound(ledgerResult.Errors.FirstOrDefault() ?? "No ledgers found for the user");
                }
                Guid existingLedgerId = ledgerResult.Value;

                var accessResult = await EnsureLedgerAccessAsync(userId, existingLedgerId);
                if (!accessResult.IsSuccess)
                {
                    return Result.Forbidden(accessResult.Errors.FirstOrDefault() ?? "User does not have access to this ledger");
                }

                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
                    return Result.NotFound("Account not found or does not belong to the user");
                }

                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, accountDto.CurrencyFractionalUnitFactor);

                var transactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(request.TransactionTypeId);
                if (transactionType is null)
                {
                    return Result.NotFound("Transaction type not found");
                }

                int centsAmountWithSign = TransactionHelper.AdjustCentsSign(centsAmount, transactionType.Title);

                var categoryResolutionResult = await ResolveCategoryForCreateAsync(userId, existingLedgerId, request);
                if (!categoryResolutionResult.IsSuccess)
                {
                    return categoryResolutionResult.Status switch
                    {
                        ResultStatus.NotFound => Result.NotFound(categoryResolutionResult.Errors.FirstOrDefault() ?? "Failed to resolve category"),
                        ResultStatus.Forbidden => Result.Forbidden(categoryResolutionResult.Errors.FirstOrDefault() ?? "Failed to resolve category"),
                        _ => Result.Error(categoryResolutionResult.Errors.FirstOrDefault() ?? "Failed to resolve category")
                    };
                }

                var (categoryId, updatedRequest) = categoryResolutionResult.Value;
                request = updatedRequest;

                var newTransaction = new Transaction(request.AccountId, existingLedgerId, request.TransactionTypeId,
                    categoryId, accountDto.CurrencyId, centsAmount, request.Date, request.Note, request.BudgetId, userId, request.BudgetCategoryId);

                Guid trId = await _transactionRepository.CreateTransactionAsync(newTransaction);

                if (request.BudgetId is not null && request.BudgetCategoryId is not null)
                {
                    var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(existingLedgerId, request.BudgetId.Value, request.BudgetCategoryId.Value);
                    if (budgetCategoryDto is null)
                    {
                        return Result.NotFound("Budget Category not found");
                    }

                    var budgetCategory = new BudgetCategory(request.BudgetId.Value,
                        budgetCategoryDto.CategoryId, budgetCategoryDto.CurrencyId, budgetCategoryDto.PlannedAmount,
                        budgetCategoryDto.CurrentAmount, budgetCategoryDto.InitialPlannedAmount);
                    budgetCategory.AddTransaction(centsAmountWithSign);

                    var budgetCategoryResult = await _budgetCategoryRepository.UpdateBudgetCategoryAsync(budgetCategoryDto.Id, budgetCategory);
                    if (!budgetCategoryResult)
                    {
                        return Result.Error("Unexpected error during updating budget category");
                    }
                }

                var account = new Account(accountDto.UserId, accountDto.AccountTypeId, accountDto.AccountTitle, accountDto.AccountBalance, accountDto.CurrencyId);
                account.AddTransaction(centsAmountWithSign);

                bool accountResult = await _accountRepository.UpdateAccountAsync(accountDto.AccountId, account);
                if (!accountResult)
                {
                    return Result.Error("Unexpected error during updating account");
                }

                return Result.Success(trId);
            });
        }

    }
}
