using Ardalis.Result;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.Categories;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionImportHandler : TransactionCommandHandlerBase, ITransactionImportHandler
    {
        private readonly IAccountQueryService _accountQueryService;
        private readonly ICategoryQueryService _categoryQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly IAccountTypeService _accountTypeService;
        private readonly ICurrencyQueryService _currencyQueryService;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<TransactionImportHandler> _logger;

        public TransactionImportHandler(
            IAccountQueryService accountQueryService,
            ICategoryQueryService categoryQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            IAccountTypeService accountTypeService,
            ICurrencyQueryService currencyQueryService,
            IUnitOfWork unitOfWork,
            IAccountRepository accountRepository,
            ICategoryRepository categoryRepository,
            ITransactionRepository transactionRepository,
            ILogger<TransactionImportHandler> logger)
            : base(unitOfWork, logger)
        {
            _accountQueryService = accountQueryService;
            _categoryQueryService = categoryQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _accountTypeService = accountTypeService;
            _currencyQueryService = currencyQueryService;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<Result<int>> HandleAsync(Guid userId, Guid ledgerId, Microsoft.AspNetCore.Http.IFormFile file)
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

            return await ExecuteInTransactionWithErrorAsync(async () =>
            {
                var transactionsToInsert = new List<Transaction>();
                int successCount = 0;

                using var stream = file.OpenReadStream();
                var imported = TransactionImportParser.ParseTransactions(stream, fileName, _logger);
                foreach (var row in imported)
                {
                    var type = allowedTypeIds.FirstOrDefault(t => t.Title.Equals(row.TransactionType, StringComparison.OrdinalIgnoreCase));
                    if (type == null)
                    {
                        _logger.LogWarning("Transaction type not allowed or not found: {Type} (Row: {@Row})", row.TransactionType, row);
                        continue;
                    }

                    var account = accounts.FirstOrDefault(a => a.AccountTitle.Equals(row.AccountName, StringComparison.OrdinalIgnoreCase));
                    if (account == null)
                    {
                        var defaultAccountType = accountTypes.FirstOrDefault();
                        var defaultCurrency = currencies.FirstOrDefault();
                        if (defaultAccountType == null || defaultCurrency == null)
                        {
                            _logger.LogWarning("Cannot create account, missing default account type or currency. (Row: {@Row})", row);
                            continue;
                        }

                        try
                        {
                            var newAccount = new Account(userId, defaultAccountType.Id, row.AccountName, 0, defaultCurrency.Id);
                            var createdAccountId = await _accountRepository.CreateAccountAsync(newAccount);

                            account = await _accountQueryService.GetAccountAsync(createdAccountId);
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
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Exception creating account: {AccountName} (Row: {@Row})", row.AccountName, row);
                            continue;
                        }
                    }

                    if (account is null)
                    {
                        _logger.LogWarning("Account resolution failed for import row. (Row: {@Row})", row);
                        continue;
                    }

                    Guid? categoryId = null;
                    var category = categories.FirstOrDefault(c => c.Title.Equals(row.Category, StringComparison.OrdinalIgnoreCase) && c.TransactionTypeId == type.Id);
                    if (category == null && !string.IsNullOrWhiteSpace(row.Category))
                    {
                        try
                        {
                            var newCategory = new Category(row.Category, ledgerId, type.Id);
                            categoryId = await _categoryRepository.CreateCategoryAsync(newCategory);

                            categories.Add(new DTOs.Models.Categories.CategoryDto
                            {
                                Id = categoryId.Value,
                                Title = row.Category,
                                LedgerId = ledgerId,
                                TransactionTypeId = type.Id
                            });
                            _logger.LogInformation("Created new category: {Category}", row.Category);
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

                    if (account.AccountId == Guid.Empty || type.Id == Guid.Empty)
                    {
                        _logger.LogWarning("Missing required fields for transaction import (Row: {@Row})", row);
                        continue;
                    }

                    var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(account.AccountId)
                        .ForUpdate().QueryFirstOrDefaultAsync();
                    if (accountDto is null)
                    {
                        _logger.LogWarning("Account currency details not found for import row. AccountId: {AccountId} (Row: {@Row})", account.AccountId, row);
                        continue;
                    }

                    int centsAmount = MoneyConverter.ConvertToCents((double)row.Amount, accountDto.CurrencyFractionalUnitFactor);
                    int centsAmountWithSign = TransactionHelper.AdjustCentsSign(centsAmount, type.Title);
                    var transaction = new Transaction(account.AccountId, ledgerId, type.Id, categoryId, accountDto.CurrencyId, centsAmountWithSign, row.Date, row.Note, null, userId, null);
                    transactionsToInsert.Add(transaction);
                    successCount++;
                }

                if (transactionsToInsert.Count > 0)
                {
                    await _transactionRepository.CreateTransactionsAsync(transactionsToInsert);
                }

                return Result.Success(successCount);
            }, "Exception during batch import of transactions");
        }
    }
}
