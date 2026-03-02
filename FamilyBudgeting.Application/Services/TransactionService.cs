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
        private readonly ITransactionCreateHandler _transactionCreateHandler;
        private readonly ITransactionUpdateHandler _transactionUpdateHandler;
        private readonly ITransactionDeleteHandler _transactionDeleteHandler;
        private readonly ITransactionTransferHandler _transactionTransferHandler;

        public TransactionService(ITransactionRepository transactionRepository, ITransactionQueryService transactionQueryService,
            IUserLedgerQueryService userLedgerQueryService, ILedgerQueryService ledgerQueryService,
            IAccountQueryService accountQueryService, ITransactionTypeQueryService transactionTypeQueryService,
            ICategoryQueryService categoryQueryService, ICurrencyQueryService currencyQueryService, IBudgetQueryService budgetQueryService,
            IBudgetCategoryQueryService budgetCategoryQueryService, IUnitOfWork unitOfWork, IAccountRepository accountRepository,
            IBudgetCategoryRepository budgetCategoryRepository, ILogger<TransactionService> logger, IAccountService accountService, 
            ICategoryService categoryService, IAccountTypeService accountTypeService, IBudgetCategoryService budgetCategoryService,
            ITransactionCreateHandler transactionCreateHandler, ITransactionUpdateHandler transactionUpdateHandler, ITransactionDeleteHandler transactionDeleteHandler,
            ITransactionTransferHandler transactionTransferHandler)
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
            _transactionCreateHandler = transactionCreateHandler;
            _transactionUpdateHandler = transactionUpdateHandler;
            _transactionDeleteHandler = transactionDeleteHandler;
            _transactionTransferHandler = transactionTransferHandler;
        }

        public async Task<Result<Guid>> CreateTransactionAsync(Guid userId, CreateTransactionRequest request)
        {
            return await _transactionCreateHandler.HandleAsync(userId, request);
        }

        public async Task<Result<bool>> UpdateTransactionAsync(Guid userId, UpdateTransactionRequest request)
        {
            return await _transactionUpdateHandler.HandleAsync(userId, request);
        }

        public async Task<Result<bool>> DeleteTransactionAsync(Guid userId, DeleteTransactionRequest request)
        {
            return await _transactionDeleteHandler.HandleAsync(userId, request);
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
            return await _transactionTransferHandler.HandleAsync(userId, request);
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
