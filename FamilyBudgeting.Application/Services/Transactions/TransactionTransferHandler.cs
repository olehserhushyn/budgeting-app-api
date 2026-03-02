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
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IUserLedgerQueryService _userLedgerQueryService;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;

        public TransactionTransferHandler(
            ILedgerQueryService ledgerQueryService,
            IUserLedgerQueryService userLedgerQueryService,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILogger<TransactionTransferHandler> logger)
        : base(unitOfWork, logger)
        {
            _ledgerQueryService = ledgerQueryService;
            _userLedgerQueryService = userLedgerQueryService;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result<Guid>> HandleAsync(Guid userId, TransferTransactionRequest request)
        {
            return await ExecuteInTransactionWithErrorAsync(async () =>
            {
                Guid existingLedgerId = request.LedgerId ?? (await _ledgerQueryService.GetUserLedgerFirstAsync(userId))?.Id ?? Guid.Empty;
                if (existingLedgerId == Guid.Empty)
                {
                    return Result.NotFound("No ledgers found for the user");
                }

                bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, existingLedgerId);
                if (!hasAccess)
                {
                    return Result.Forbidden("User does not have access to this ledger");
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

                if (destAccountDto is null)
                {
                    return Result.NotFound("Destination account not found");
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
