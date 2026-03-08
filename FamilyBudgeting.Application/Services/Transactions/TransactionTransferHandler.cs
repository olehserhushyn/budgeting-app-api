using Ardalis.Result;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Accounts;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionTransferHandler : TransactionCommandHandlerBase, ITransactionTransferHandler
    {
        private readonly ITransactionAccessPolicy _transactionAccessPolicy;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;

        public TransactionTransferHandler(
            ITransactionAccessPolicy transactionAccessPolicy,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransactionTransferHandler> logger)
        : base(unitOfWork, logger)
        {
            _transactionAccessPolicy = transactionAccessPolicy;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result<Guid>> HandleAsync(Guid userId, TransferTransactionRequest request)
        {
            return await ExecuteInTransactionWithErrorAsync(async () =>
            {
                var ledgerResult = await _transactionAccessPolicy.ResolveLedgerAsync(userId, request.LedgerId);
                if (!ledgerResult.IsSuccess)
                {
                    return Result.NotFound(ledgerResult.Errors.FirstOrDefault() ?? "No ledgers found for the user");
                }

                Guid existingLedgerId = ledgerResult.Value;

                var accessResult = await _transactionAccessPolicy.EnsureLedgerAccessAsync(userId, existingLedgerId);
                if (!accessResult.IsSuccess)
                {
                    return Result.Forbidden(accessResult.Errors.FirstOrDefault() ?? "User does not have access to this ledger");
                }

                if (request.SourceAccountId == request.DestinationAccountId)
                {
                    return Result.Invalid(new ValidationError
                    {
                        Identifier = nameof(request.DestinationAccountId),
                        ErrorMessage = "Source and destination accounts must be different"
                    });
                }

                var sourceAccountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.SourceAccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (sourceAccountDto is null || sourceAccountDto.UserId != userId)
                {
                    return Result.NotFound("Source account not found or does not belong to the user");
                }

                var destAccountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(request.DestinationAccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (destAccountDto is null || destAccountDto.UserId != userId)
                {
                    return Result.NotFound("Destination account not found or does not belong to the user");
                }

                if (sourceAccountDto.CurrencyId != destAccountDto.CurrencyId)
                {
                    return Result.Error("Currency mismatch between source and destination accounts");
                }

                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, sourceAccountDto.CurrencyFractionalUnitFactor);
                if (sourceAccountDto.AccountBalance < centsAmount)
                {
                    return Result.Error("Insufficient funds in source account");
                }

                var transferTypeId = TransactionTypes.TransferId;
                if (transferTypeId == Guid.Empty)
                {
                    var types = await _transactionTypeQueryService.GetTransactionsTypesAsync();
                    TransactionTypes.Initialize(types);
                    transferTypeId = TransactionTypes.TransferId;
                }

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

                var sourceAccount = new Account(sourceAccountDto.UserId, sourceAccountDto.AccountTypeId, sourceAccountDto.AccountTitle, sourceAccountDto.AccountBalance, sourceAccountDto.CurrencyId);
                sourceAccount.AddTransaction(-centsAmount);
                bool sourceResult = await _accountRepository.UpdateAccountAsync(sourceAccountDto.AccountId, sourceAccount);
                if (!sourceResult)
                {
                    return Result.Error("Unexpected error during updating source account");
                }

                var destAccount = new Account(destAccountDto.UserId, destAccountDto.AccountTypeId, destAccountDto.AccountTitle, destAccountDto.AccountBalance, destAccountDto.CurrencyId);
                destAccount.AddTransaction(centsAmount);
                bool destResult = await _accountRepository.UpdateAccountAsync(destAccountDto.AccountId, destAccount);
                if (!destResult)
                {
                    return Result.Error("Unexpected error during updating destination account");
                }

                return Result.Success(transferId);
            }, "Transfer failed");
        }

    }
}
