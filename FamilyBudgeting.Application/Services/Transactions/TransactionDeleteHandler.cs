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
    public class TransactionDeleteHandler : TransactionCommandHandlerBase, ITransactionDeleteHandler
    {
        private readonly ITransactionAccessPolicy _transactionAccessPolicy;
        private readonly ITransactionQueryService _transactionQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;

        public TransactionDeleteHandler(
            ITransactionAccessPolicy transactionAccessPolicy,
            ITransactionQueryService transactionQueryService,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransactionDeleteHandler> logger)
        : base(unitOfWork, logger)
        {
            _transactionAccessPolicy = transactionAccessPolicy;
            _transactionQueryService = transactionQueryService;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result<bool>> HandleAsync(Guid userId, DeleteTransactionRequest request)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var existingTransaction = await _transactionQueryService.GetTransactionById(request.TransactionId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();
                if (existingTransaction is null)
                {
                    return Result.NotFound("Unable to delete transaction. Transaction was not found.");
                }

                var accessResult = await _transactionAccessPolicy.EnsureLedgerAccessAsync(userId, existingTransaction.LedgerId);
                if (!accessResult.IsSuccess)
                {
                    return Result.Forbidden(accessResult.Errors.FirstOrDefault() ?? "User does not have access to this ledger");
                }

                if (request.LedgerId != existingTransaction.LedgerId)
                {
                    return Result.Invalid(new ValidationError
                    {
                        Identifier = nameof(request.LedgerId),
                        ErrorMessage = "Ledger mismatch for transaction delete request"
                    });
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
            }, "Error deleting transaction");
        }

    }
}
