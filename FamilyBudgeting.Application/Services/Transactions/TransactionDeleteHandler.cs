using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionDeleteHandler : ITransactionDeleteHandler
    {
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly ITransactionQueryService _transactionQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionDeleteHandler> _logger;

        public TransactionDeleteHandler(
            IUserLedgerQueryService userLedgerQueryService,
            ITransactionQueryService transactionQueryService,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransactionDeleteHandler> logger)
        {
            _userLedgerQueryService = userLedgerQueryService;
            _transactionQueryService = transactionQueryService;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> HandleAsync(Guid userId, DeleteTransactionRequest request)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, request.LedgerId);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
                }

                var existingTransaction = await _transactionQueryService.GetTransactionById(request.TransactionId).QueryFirstOrDefaultAsync();
                if (existingTransaction is null)
                {
                    return Result.NotFound("Unable to update transaction. Transaction was not found.");
                }

                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(existingTransaction.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
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
                var tranResult = await _transactionRepository.UpdateTransactionAsync(request.TransactionId, transaction);
                if (!tranResult)
                {
                    return Result.Error("Unexpected error during deleting transaction");
                }

                var account = new Account(accountDto.UserId, accountDto.AccountTypeId, accountDto.AccountTitle, accountDto.AccountBalance, accountDto.CurrencyId);
                account.RemoveTransaction(existingTransactionType.Title == TransactionTypes.Expense ? existingTransaction.Amount * -1 : existingTransaction.Amount);

                bool accountResult = await _accountRepository.UpdateAccountAsync(accountDto.AccountId, account);
                if (!accountResult)
                {
                    return Result.Error("Unexpected error during updating account");
                }

                return Result.Success(tranResult);
            });
        }

        private async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> operation)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await operation();
                if (result.IsSuccess)
                    await _unitOfWork.CommitTransactionAsync();
                else
                    await _unitOfWork.RollbackTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting transaction");
                throw;
            }
        }
    }
}
