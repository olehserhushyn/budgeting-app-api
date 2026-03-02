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
    public class TransactionCreateHandler : ITransactionCreateHandler
    {
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ICategoryQueryService _categoryQueryService;
        private readonly ICurrencyQueryService _currencyQueryService;
        private readonly IBudgetQueryService _budgetQueryService;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly IUnitOfWork _unitOfWork;
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
            IAccountRepository accountRepository,
            IBudgetCategoryRepository budgetCategoryRepository,
            ITransactionRepository transactionRepository,
            ICategoryService categoryService,
            IBudgetCategoryService budgetCategoryService)
        {
            _userLedgerQueryService = userLedgerQueryService;
            _ledgerQueryService = ledgerQueryService;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _categoryQueryService = categoryQueryService;
            _currencyQueryService = currencyQueryService;
            _budgetQueryService = budgetQueryService;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _unitOfWork = unitOfWork;
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

        private async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> operation)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await operation();

                if (result.IsSuccess)
                {
                    await _unitOfWork.CommitTransactionAsync();
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }

                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<Result<Guid>> ResolveLedgerForCreateAsync(Guid userId, Guid? ledgerId)
        {
            if (ledgerId.HasValue)
            {
                return Result.Success(ledgerId.Value);
            }

            var firstLedger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId);
            if (firstLedger is null)
            {
                return Result.NotFound("No ledgers found for the user");
            }

            return Result.Success(firstLedger.Id);
        }

        private async Task<Result> EnsureLedgerAccessAsync(Guid userId, Guid ledgerId)
        {
            bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, ledgerId);
            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this ledger");
            }

            return Result.Success();
        }

        private async Task<Result<(Guid? CategoryId, CreateTransactionRequest Request)>> ResolveCategoryForCreateAsync(
            Guid userId,
            Guid existingLedgerId,
            CreateTransactionRequest request)
        {
            Guid? categoryId = request.CategoryId;

            if (categoryId == null && !string.IsNullOrWhiteSpace(request.BudgetCategoryTitle) && request.BudgetId is not null)
            {
                var existingBudgetCategories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(existingLedgerId, request.BudgetId.Value);
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
                        var createLedgerCategoryReq = new CreateLedgerCategoryRequest(existingLedgerId, request.BudgetCategoryTitle, request.TransactionTypeId);
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

                    var createdBudgetCategories = await _budgetCategoryQueryService.GetBudgetCategoriesDetailedAsync(existingLedgerId, request.BudgetId.Value);
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
                    var createLedgerCategoryReq = new CreateLedgerCategoryRequest(existingLedgerId, request.CategoryTitle, request.TransactionTypeId);
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
                var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(existingLedgerId, request.BudgetId.Value, request.BudgetCategoryId.Value);
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
