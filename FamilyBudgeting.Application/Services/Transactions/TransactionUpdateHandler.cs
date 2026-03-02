using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.BudgetCategories;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionUpdateHandler : TransactionCommandHandlerBase, ITransactionUpdateHandler
    {
        private readonly ITransactionQueryService _transactionQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IBudgetCategoryQueryService _budgetCategoryQueryService;
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;
        private readonly IAccountRepository _accountRepository;

        public TransactionUpdateHandler(
            ITransactionQueryService transactionQueryService,
            IAccountQueryService accountQueryService,
            ILedgerQueryService ledgerQueryService,
            IUserLedgerQueryService userLedgerQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionRepository transactionRepository,
            IBudgetCategoryQueryService budgetCategoryQueryService,
            IBudgetCategoryRepository budgetCategoryRepository,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransactionUpdateHandler> logger)
        : base(unitOfWork, logger)
        {
            _transactionQueryService = transactionQueryService;
            _accountQueryService = accountQueryService;
            _ledgerQueryService = ledgerQueryService;
            _userLedgerQueryService = userLedgerQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionRepository = transactionRepository;
            _budgetCategoryQueryService = budgetCategoryQueryService;
            _budgetCategoryRepository = budgetCategoryRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result<bool>> HandleAsync(Guid userId, UpdateTransactionRequest request)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var existingTransaction = await _transactionQueryService.GetTransactionById(request.TransactionId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (existingTransaction is null)
                {
                    return Result.NotFound("Unable to update transaction. Transaction was not found.");
                }

                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
                    return Result.NotFound("Account not found or does not belong to the user");
                }

                var existingLedgerId = request.LedgerId ?? (await _ledgerQueryService.GetUserLedgerFirstAsync(userId))?.Id;
                if (existingLedgerId == null)
                {
                    return Result.NotFound("No ledgers found for the user");
                }

                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, existingLedgerId.Value);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
                }

                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, accountDto.CurrencyFractionalUnitFactor);
                var transactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(request.TransactionTypeId);
                var existingTransactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(existingTransaction.TransactionTypeId);

                if (transactionType is null || existingTransactionType is null)
                {
                    return Result.NotFound("Transaction type not found");
                }

                var newTransaction = new Transaction(
                    request.AccountId, existingLedgerId.Value, request.TransactionTypeId,
                    request.CategoryId, accountDto.CurrencyId, centsAmount,
                    request.Date, request.Note, request.BudgetId, userId, request.BudgetCategoryId);

                var tranResult = await _transactionRepository.UpdateTransactionAsync(request.TransactionId, newTransaction);
                if (!tranResult)
                {
                    return Result.Error("Unexpected error during updating transaction");
                }

                if (request.BudgetId is not null && request.BudgetCategoryId is not null)
                {
                    var budgetCategoryDto = await _budgetCategoryQueryService.GetBudgetCategoryAsync(
                        existingLedgerId.Value, request.BudgetId.Value, request.BudgetCategoryId.Value);

                    if (budgetCategoryDto is null)
                    {
                        return Result.NotFound("Budget Category not found");
                    }

                    var budgetCategory = new BudgetCategory(
                        request.BudgetId.Value, budgetCategoryDto.CategoryId,
                        budgetCategoryDto.CurrencyId, budgetCategoryDto.PlannedAmount,
                        budgetCategoryDto.CurrentAmount, budgetCategoryDto.InitialPlannedAmount);

                    budgetCategory.AddTransaction(TransactionHelper.AdjustCentsSign(centsAmount, transactionType.Title));

                    var budgetCategoryResult = await _budgetCategoryRepository.UpdateBudgetCategoryAsync(budgetCategoryDto.Id, budgetCategory);
                    if (!budgetCategoryResult)
                    {
                        return Result.Error("Unexpected error during updating budget category");
                    }
                }

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
                    return Result.Error("Unexpected error during updating account");
                }

                return Result.Success(true);
            }, "Error updating transaction");
        }

    }
}
