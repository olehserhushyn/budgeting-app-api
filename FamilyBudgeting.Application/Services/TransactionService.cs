using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Domain.Data.Transactions;
using Microsoft.Extensions.Logging;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
// using FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories; // not used - budget category DTO is in Categories namespace

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionQueryService _transactionQueryService;
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
        private readonly ILogger<TransactionService> _logger;
        private readonly IAccountService _accountService;
        private readonly ICategoryService _categoryService;
        private readonly IAccountTypeService _accountTypeService;
        private readonly IBudgetCategoryService _budgetCategoryService;

        public TransactionService(ITransactionRepository transactionRepository, ITransactionQueryService transactionQueryService,
            IUserLedgerQueryService userLedgerQueryService, ILedgerQueryService ledgerQueryService,
            IAccountQueryService accountQueryService, ITransactionTypeQueryService transactionTypeQueryService,
            ICategoryQueryService categoryQueryService, ICurrencyQueryService currencyQueryService, IBudgetQueryService budgetQueryService,
            IBudgetCategoryQueryService budgetCategoryQueryService, IUnitOfWork unitOfWork, IAccountRepository accountRepository,
            IBudgetCategoryRepository budgetCategoryRepository, ILogger<TransactionService> logger, IAccountService accountService, 
            ICategoryService categoryService, IAccountTypeService accountTypeService, IBudgetCategoryService budgetCategoryService)
        {
            _transactionRepository = transactionRepository;
            _transactionQueryService = transactionQueryService;
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
            _logger = logger;
            _accountService = accountService;
            _categoryService = categoryService;
            _accountTypeService = accountTypeService;
            _budgetCategoryService = budgetCategoryService;
        }

        public async Task<Result<Guid>> CreateTransactionAsync(Guid userId, CreateTransactionRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var ledgerResult = await ResolveLedgerForCreateAsync(userId, request.LedgerId);
                if (!ledgerResult.IsSuccess)
                {
                    return await RollbackCreateTransactionAsync(ledgerResult.Status, ledgerResult.Errors.FirstOrDefault() ?? "No ledgers found for the user");
                }
                Guid existingLedgerId = ledgerResult.Value;

                var accessResult = await EnsureLedgerAccessAsync(userId, existingLedgerId);
                if (!accessResult.IsSuccess)
                {
                    return await RollbackCreateTransactionAsync(accessResult.Status, accessResult.Errors.FirstOrDefault() ?? "User does not have access to this ledger");
                }

                // account-currency
                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.NotFound("Account not found or does not belong to the user");
                }

                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, accountDto.CurrencyFractionalUnitFactor);

                var transactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(request.TransactionTypeId);

                if (transactionType is null)
                {
                    return await RollbackCreateTransactionAsync(ResultStatus.NotFound, "Transaction type not found");
                }

                int centsAmountWithSign = TransactionHelper.AdjustCentsSign(centsAmount, transactionType.Title);

                var categoryResolutionResult = await ResolveCategoryForCreateAsync(userId, existingLedgerId, request);
                if (!categoryResolutionResult.IsSuccess)
                {
                    return await RollbackCreateTransactionAsync(
                        categoryResolutionResult.Status,
                        categoryResolutionResult.Errors.FirstOrDefault() ?? "Failed to resolve category");
                }

                var (categoryId, updatedRequest) = categoryResolutionResult.Value;
                request = updatedRequest;

                // transaction
                var newTransaction = new Transaction(request.AccountId, existingLedgerId, request.TransactionTypeId,
                    categoryId, accountDto.CurrencyId, centsAmount, request.Date, request.Note, request.BudgetId, userId, request.BudgetCategoryId);

                Guid trId = await _transactionRepository.CreateTransactionAsync(newTransaction);

                if (request.BudgetId is not null && request.BudgetCategoryId is not null)
                {
                    // check if budget exists
                    var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(existingLedgerId, request.BudgetId.Value, request.BudgetCategoryId.Value);
                    if (budgetCategoryDto is null)
                    {
                        return await RollbackCreateTransactionAsync(ResultStatus.NotFound, "Budget Category not found");
                    }

                    BudgetCategory budgetCategory = new BudgetCategory(request.BudgetId.Value, 
                        budgetCategoryDto.CategoryId, budgetCategoryDto.CurrencyId, budgetCategoryDto.PlannedAmount, 
                        budgetCategoryDto.CurrentAmount, budgetCategoryDto.InitialPlannedAmount);
                    budgetCategory.AddTransaction(centsAmountWithSign);

                    var budgetCategoryResult = await _budgetCategoryRepository.UpdateBudgetCategoryAsync(budgetCategoryDto.Id, budgetCategory);

                    if (!budgetCategoryResult)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result.Error("Unexpected error during updating budget category");
                    }
                }

                // adjust account balance
                var account = new Account(accountDto.UserId, accountDto.AccountTypeId, accountDto.AccountTitle, accountDto.AccountBalance, accountDto.CurrencyId);
                account.AddTransaction(centsAmountWithSign);

                bool accountResult = await _accountRepository.UpdateAccountAsync(accountDto.AccountId, account);
                if (!accountResult)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Unexpected error during updating account");
                }

                await _unitOfWork.CommitTransactionAsync();

                return Result.Success(trId);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }


        private async Task<Result<Guid>> RollbackCreateTransactionAsync(ResultStatus status, string message)
        {
            await _unitOfWork.RollbackTransactionAsync();

            return status switch
            {
                ResultStatus.NotFound => Result.NotFound(message),
                ResultStatus.Forbidden => Result.Forbidden(message),
                ResultStatus.Invalid => Result.Invalid(new[] { new ValidationError { ErrorMessage = message } }),
                _ => Result.Error(message)
            };
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
                        if (createCatRes.Status != Ardalis.Result.ResultStatus.Ok)
                        {
                            return Result.Error("Failed to create base category for budget category");
                        }
                    }

                    var createBudgetCategoryReq = new CreateBudgetCategoryRequest(request.BudgetCategoryTitle, request.BudgetId.Value, currency.Id, 0, request.TransactionTypeId);
                    var createBudgetCatRes = await _budgetCategoryService.CreateBudgetCategoryAsync(userId, createBudgetCategoryReq);
                    if (createBudgetCatRes.Status != Ardalis.Result.ResultStatus.Ok)
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
                    if (createCatRes.Status != Ardalis.Result.ResultStatus.Ok)
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

        public async Task<Result<bool>> UpdateTransactionAsync(Guid userId, UpdateTransactionRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get and lock transaction first
                var existingTransaction = await _transactionQueryService.GetTransactionById(request.TransactionId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (existingTransaction is null)
                {
                    return Result.NotFound("Unable to update transaction. Transaction was not found.");
                }

                // Get and lock account (since we'll update it)
                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.NotFound("Account not found or does not belong to the user");
                }

                // Rest of validation checks (non-locking)
                var existingLedgerId = request.LedgerId ??
                    (await _ledgerQueryService.GetUserLedgerFirstAsync(userId))?.Id;

                if (existingLedgerId == null)
                {
                    return Result.NotFound("No ledgers found for the user");
                }

                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, existingLedgerId.Value);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
                }

                // Transaction processing
                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, accountDto.CurrencyFractionalUnitFactor);
                var transactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(request.TransactionTypeId);
                var existingTransactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(existingTransaction.TransactionTypeId);

                if (transactionType is null || existingTransactionType is null)
                {
                    return Result.NotFound("Transaction type not found");
                }

                // Update transaction
                var newTransaction = new Transaction(
                    request.AccountId, existingLedgerId.Value, request.TransactionTypeId,
                    request.CategoryId, accountDto.CurrencyId, centsAmount,
                    request.Date, request.Note, request.BudgetId, userId, request.BudgetCategoryId);

                var tranResult = await _transactionRepository.UpdateTransactionAsync(request.TransactionId, newTransaction);
                if (!tranResult)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Unexpected error during updating transaction");
                }

                // Update budget category if needed
                if (request.BudgetId is not null && request.BudgetCategoryId is not null)
                {
                    var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(
                        existingLedgerId.Value, request.BudgetId.Value, request.BudgetCategoryId.Value);

                    if (budgetCategoryDto is null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result.NotFound("Budget Category not found");
                    }

                    var budgetCategory = new BudgetCategory(
                        request.BudgetId.Value, budgetCategoryDto.CategoryId,
                        budgetCategoryDto.CurrencyId, budgetCategoryDto.PlannedAmount,
                        budgetCategoryDto.CurrentAmount, budgetCategoryDto.InitialPlannedAmount);

                    budgetCategory.AddTransaction(
                        TransactionHelper.AdjustCentsSign(centsAmount, transactionType.Title));

                    var budgetCategoryResult = await _budgetCategoryRepository.UpdateBudgetCategoryAsync(
                        budgetCategoryDto.Id, budgetCategory);

                    if (!budgetCategoryResult)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result.Error("Unexpected error during updating budget category");
                    }
                }

                // Update account balance
                var account = new Account(
                    accountDto.UserId, accountDto.AccountTypeId,
                    accountDto.AccountTitle, accountDto.AccountBalance,
                    accountDto.CurrencyId);

                account.UpdateTransaction(
                    TransactionHelper.AdjustCentsSign(existingTransaction.Amount, existingTransactionType.Title),
                    TransactionHelper.AdjustCentsSign(centsAmount, transactionType.Title));

                bool accountResult = await _accountRepository.UpdateAccountAsync(accountDto.AccountId, account);

                if (!accountResult)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Unexpected error during updating account");
                }

                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating transaction");
                throw;
            }
        }

        public async Task<Result<bool>> DeleteTransactionAsync(Guid userId, DeleteTransactionRequest request)
        {

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // access
                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, request.LedgerId);

                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
                }

                // transaction
                var existingTransaction = await _transactionQueryService.GetTransactionById(request.TransactionId).QueryFirstOrDefaultAsync();

                if (existingTransaction is null)
                {
                    return Result.NotFound("Unable to update transaction. Transaction was not found.");
                }

                // account-currency
                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(existingTransaction.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.NotFound("Account not found or does not belong to the user");
                }

                var existingTransactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(existingTransaction.TransactionTypeId);

                if (existingTransactionType is null)
                {
                    return Result.NotFound("Transaction type not found");
                }

                var transaction = new Transaction(existingTransaction.AccountId, existingTransaction.LedgerId, existingTransaction.TransactionTypeId, existingTransaction.CategoryId,
                    existingTransaction.CurrencyId, existingTransaction.Amount, existingTransaction.Date, existingTransaction.Note, existingTransaction.BudgetId, existingTransaction.UserId, existingTransaction.BudgetCategoryId);
                
                transaction.Delete();
                var tranResult = await _transactionRepository.UpdateTransactionAsync(existingTransactionType.Id, transaction);

                if (!tranResult)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Unexpected error during creating transaction");
                }

                // adjust account balance
                var account = new Account(accountDto.UserId, accountDto.AccountTypeId, accountDto.AccountTitle, accountDto.AccountBalance, accountDto.CurrencyId);
                account.RemoveTransaction(existingTransactionType.Title == TransactionTypes.Expense ? existingTransaction.Amount * -1 : existingTransaction.Amount);

                bool accountResult = await _accountRepository.UpdateAccountAsync(accountDto.AccountId, account);
                if (!accountResult)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result.Error("Unexpected error during updating account");
                }

                await _unitOfWork.CommitTransactionAsync();

                return Result.Success(tranResult);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Result<PaginatedTransactionListResponse>> GetTransactionsFromLedgerAsync(GetTransactionsFromLedgerRequest request)
        {
            // If ledger is specified, check access
            if (request.LedgerId.HasValue)
            {
                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(request.UserId, request.LedgerId.Value);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
                }
            }

            var (items, totalCount) = await _transactionQueryService.GetTransactionListPaginatedAsync(
                request.LedgerId,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.UserId,
                request.Page,
                request.PageSize
            );

            if (items is null)
            {
                return Result<PaginatedTransactionListResponse>.NotFound("Transactions not found");
            }

            var summary = await _transactionQueryService.GetTransactionSummaryAsync(request.LedgerId,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                request.BudgetId,
                request.CategoryId,
                request.BudgetCategoryId,
                request.UserId);

            var response = new PaginatedTransactionListResponse
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                Summary = summary
            };

            return Result<PaginatedTransactionListResponse>.Success(response);
        }

        public async Task<Result<GetCreateTransactionPageDataResponse>> GetCreateTransactionPageDataAsync(Guid userId, Guid? budgetId, Guid? ledgerId)
        {
            //var ledgers = await _ledgerQueryService.GetUserLedgersAsync(userId);
            var accounts = await _accountQueryService.GetAccountsAsync(userId);
            //var currencies = await _currencyQueryService.GetCurrenciesFromLedgerAsync();
            var transactionTypes = await _transactionTypeQueryService.GetTransactionsTypesAsync();

            Guid existingLedgerId = Guid.Empty;

            if (ledgerId is null)
            {
                var firstLedger = await _ledgerQueryService.GetUserLedgerFirstAsync(userId);
                if (firstLedger is not null)
                {
                    existingLedgerId = firstLedger.Id;
                }
                else
                {
                    return Result.NotFound("No ledgers found for the user");
                }
            }
            else
            {
                existingLedgerId = ledgerId.Value;
            }

            var budgets = await _budgetQueryService.GetBudgetsFromLedgerAsync(existingLedgerId);

            Dictionary<Guid, CategoryWithTypeDto> budgetCategories = new Dictionary<Guid, CategoryWithTypeDto>();
            Dictionary<Guid, CategoryWithTypeDto> transactionCategories = new Dictionary<Guid, CategoryWithTypeDto>();

            if (budgetId is not null)
            {
                var categories = await _budgetCategoryQueryService.GetBudgetCategoriesAsync(existingLedgerId, budgetId.Value);
                budgetCategories = categories.ToDictionary(x => x.Id, x => new CategoryWithTypeDto {
                    Title = x.CategoryName,
                    TransactionTypeId = x.TransactionTypeId,
                    TransactionTypeTitle = x.TransactionTypeTitle
                });
            }
            else
            {
                var categories = await _categoryQueryService.GetCategoriesAsync(existingLedgerId);
                transactionCategories = categories.ToDictionary(x => x.Id, x => new CategoryWithTypeDto {
                    Title = x.Title,
                    TransactionTypeId = x.TransactionTypeId,
                    TransactionTypeTitle = "" // If you have the title, set it here, otherwise leave blank
                });
            }

            var response = new GetCreateTransactionPageDataResponse
            {
                //Ledgers = ledgers?.ToDictionary(x => x.Id, x => x.Title),
                Accounts = accounts?.ToDictionary(x => x.AccountId, x => string.Join('|', x.AccountTitle, x.CurrencySymbol)),
                TransactionCategories = transactionCategories,
                BudgetCategories = budgetCategories,
                //Currencies = currencies?.ToDictionary(x => x.Id, x => string.Join(' ', x.Code, x.Symbol)),
                TransactionTypes = transactionTypes?.ToDictionary(x => x.Id, x=> x.Title),
                Budgets = budgets?.ToDictionary(x => x.Id, x => $"{x.StartDate.ToString("yyyy-MM-dd")}-{x.EndDate.ToString("yyyy-MM-dd")}" ),
            };

            return Result.Success(response);
        }

        public async Task<Result<Guid>> TransferAsync(Guid userId, TransferTransactionRequest request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Determine ledger
                Guid existingLedgerId = request.LedgerId ?? (await _ledgerQueryService.GetUserLedgerFirstAsync(userId))?.Id ?? Guid.Empty;
                if (existingLedgerId == Guid.Empty)
                    return Result.NotFound("No ledgers found for the user");

                // Access check
                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, existingLedgerId);
                if (!hasAccess)
                    return Result.Forbidden("User does not have access to this ledger");

                // Source account
                var sourceAccountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.SourceAccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (sourceAccountDto is null || sourceAccountDto.UserId != userId)
                    return Result.NotFound("Source account not found or does not belong to the user");

                // Destination account
                var destAccountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.DestinationAccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (destAccountDto is null)
                    return Result.NotFound("Destination account not found");

                // Currency check
                if (sourceAccountDto.CurrencyId != destAccountDto.CurrencyId)
                    return Result.Error("Currency mismatch between source and destination accounts");

                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, sourceAccountDto.CurrencyFractionalUnitFactor);
                if (sourceAccountDto.AccountBalance < centsAmount)
                    return Result.Error("Insufficient funds in source account");

                // Get transfer type id
                var transferTypeId = TransactionTypes.TransferId;
                if (transferTypeId == Guid.Empty)
                {
                    var types = await _transactionTypeQueryService.GetTransactionsTypesAsync();
                    TransactionTypes.Initialize(types);
                    transferTypeId = TransactionTypes.TransferId;
                }

                // Create withdrawal transaction (source)
                var transfer = new Transaction(
                    request.SourceAccountId,
                    existingLedgerId,
                    transferTypeId,
                    null,
                    sourceAccountDto.CurrencyId,
                    centsAmount,
                    request.Date,
                    request.Note,
                    request.BudgetId,
                    userId,
                    request.BudgetCategoryId
                );
                Guid transferId = await _transactionRepository.CreateTransactionAsync(transfer);

                // Update source account
                var sourceAccount = new Account(sourceAccountDto.UserId, sourceAccountDto.AccountTypeId, sourceAccountDto.AccountTitle, sourceAccountDto.AccountBalance, sourceAccountDto.CurrencyId);
                sourceAccount.AddTransaction(-centsAmount);
                bool sourceResult = await _accountRepository.UpdateAccountAsync(sourceAccountDto.AccountId, sourceAccount);

                // Update destination account
                var destAccount = new Account(destAccountDto.UserId, destAccountDto.AccountTypeId, destAccountDto.AccountTitle, destAccountDto.AccountBalance, destAccountDto.CurrencyId);
                destAccount.AddTransaction(centsAmount);
                bool destResult = await _accountRepository.UpdateAccountAsync(destAccountDto.AccountId, destAccount);

                await _unitOfWork.CommitTransactionAsync();

                // Return withdrawal transaction id as the main reference
                return Result.Success(transferId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result.Error($"Transfer failed: {ex.Message}");
            }
        }

        public async Task<Result<int>> ImportTransactionsAsync(Guid userId, Guid ledgerId, Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Result.Error("No file uploaded");

            var fileName = file.FileName;
            if (!(fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
                  fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                  fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Unsupported file type: {FileName}", fileName);
                return Result.Error("Unsupported file type. Only .csv, .xlsx, .xls are allowed.");
            }

            var accounts = (await _accountQueryService.GetAccountsAsync(userId)).ToList();
            var categories = (await _categoryQueryService.GetCategoriesAsync(ledgerId)).ToList();
            var transactionTypes = (await _transactionTypeQueryService.GetTransactionsTypesAsync()).ToList();
            var accountTypes = (await _accountTypeService.GetAccountTypesAsync()).Value?.ToList() ?? new List<DTOs.Models.AccountTypes.AccountTypeDto>();
            var currencies = (await _currencyQueryService.GetCurrenciesFromLedgerAsync()).ToList();

            var allowedTypes = new[] { TransactionTypes.Income, TransactionTypes.Expense };
            var allowedTypeIds = transactionTypes.Where(t => allowedTypes.Contains(t.Title, StringComparer.OrdinalIgnoreCase)).ToList();

            var transactionsToInsert = new List<Transaction>();
            int successCount = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var imported = Utilities.TransactionImportParser.ParseTransactions(stream, fileName, _logger);
                    foreach (var row in imported)
                    {
                        // Map transaction type
                        var type = allowedTypeIds.FirstOrDefault(t => t.Title.Equals(row.TransactionType, StringComparison.OrdinalIgnoreCase));
                        if (type == null)
                        {
                            _logger.LogWarning("Transaction type not allowed or not found: {Type} (Row: {@Row})", row.TransactionType, row);
                            continue;
                        }
                        // Map account
                        var account = accounts.FirstOrDefault(a => a.AccountTitle.Equals(row.AccountName, StringComparison.OrdinalIgnoreCase));
                        if (account == null)
                        {
                            // Create account if not found
                            var defaultAccountType = accountTypes.FirstOrDefault();
                            var defaultCurrency = currencies.FirstOrDefault();
                            if (defaultAccountType == null || defaultCurrency == null)
                            {
                                _logger.LogWarning("Cannot create account, missing default account type or currency. (Row: {@Row})", row);
                                continue;
                            }
                            var createAccountReq = new DTOs.Requests.Accounts.CreateAccountRequest(
                                defaultAccountType.Id,
                                row.AccountName,
                                0,
                                defaultCurrency.Id
                            );
                            try
                            {
                                var createResult = await _accountService.CreateAccountAsync(userId, createAccountReq);
                                if (createResult.Status == Ardalis.Result.ResultStatus.Ok)
                                {
                                    account = (await _accountQueryService.GetAccountsAsync(userId)).FirstOrDefault(a => a.AccountTitle.Equals(row.AccountName, StringComparison.OrdinalIgnoreCase));
                                    if (account != null)
                                    {
                                        accounts.Add(account);
                                        _logger.LogInformation("Created new account: {AccountName}", row.AccountName);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Account creation succeeded but account not found after creation: {AccountName}", row.AccountName);
                                        continue;
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to create account: {AccountName} (Row: {@Row})", row.AccountName, row);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Exception creating account: {AccountName} (Row: {@Row})", row.AccountName, row);
                                continue;
                            }
                        }
                        // Map category (optional)
                        Guid? categoryId = null;
                        var category = categories.FirstOrDefault(c => c.Title.Equals(row.Category, StringComparison.OrdinalIgnoreCase) && c.TransactionTypeId == type.Id);
                        if (category == null && !string.IsNullOrWhiteSpace(row.Category))
                        {
                            // Create category if not found
                            var createCategoryReq = new DTOs.Requests.Categories.CreateLedgerCategoryRequest(
                                ledgerId,
                                row.Category,
                                type.Id
                            );
                            try
                            {
                                var createCatResult = await _categoryService.CreateLedgerCategoryAsync(createCategoryReq);
                                if (createCatResult.Status == Ardalis.Result.ResultStatus.Ok)
                                {
                                    categoryId = createCatResult.Value;
                                    // Add to local list for future lookups
                                    categories.Add(new DTOs.Models.Categories.CategoryDto
                                    {
                                        Id = categoryId.Value,
                                        Title = row.Category,
                                        LedgerId = ledgerId,
                                        TransactionTypeId = type.Id
                                    });
                                    _logger.LogInformation("Created new category: {Category}", row.Category);
                                }
                                else
                                {
                                    _logger.LogWarning("Failed to create category: {Category} (Row: {@Row})", row.Category, row);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Exception creating category: {Category} (Row: {@Row})", row.Category, row);
                                continue;
                            }
                        }
                        else if (category != null)
                        {
                            categoryId = category.Id;
                        }
                        // Only import if required fields are present
                        if (account.AccountId == Guid.Empty || type.Id == Guid.Empty)
                        {
                            _logger.LogWarning("Missing required fields for transaction import (Row: {@Row})", row);
                            continue;
                        }
                        // Prepare Transaction entity for batch insert
                        var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(account.AccountId)
                            .ForUpdate().QueryFirstOrDefaultAsync();
                        int centsAmount = MoneyConverter.ConvertToCents((double)row.Amount, accountDto.CurrencyFractionalUnitFactor);
                        int centsAmountWithSign = TransactionHelper.AdjustCentsSign(centsAmount, type.Title);
                        var transaction = new Transaction(
                            account.AccountId,
                            ledgerId,
                            type.Id,
                            categoryId,
                            accountDto.CurrencyId,
                            centsAmountWithSign,
                            row.Date,
                            row.Note,
                            null, // BudgetId
                            userId,
                            null // BudgetCategoryId
                        );
                        transactionsToInsert.Add(transaction);
                        successCount++;
                    }
                }
                // Batch insert all valid transactions
                if (transactionsToInsert.Count > 0)
                {
                    await _transactionRepository.CreateTransactionsAsync(transactionsToInsert);
                }
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(successCount);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Exception during batch import of transactions");
                return Result.Error($"Exception during import: {ex.Message}");
            }
        }
    }
}
