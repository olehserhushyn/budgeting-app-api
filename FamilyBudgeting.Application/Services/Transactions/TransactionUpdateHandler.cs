using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
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
        private readonly ITransactionAccessPolicy _transactionAccessPolicy;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionPostingPolicy _transactionPostingPolicy;

        public TransactionUpdateHandler(
            ITransactionQueryService transactionQueryService,
            IAccountQueryService accountQueryService,
            ITransactionAccessPolicy transactionAccessPolicy,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionRepository transactionRepository,
            ITransactionPostingPolicy transactionPostingPolicy,
            IUnitOfWork unitOfWork,
            ILogger<TransactionUpdateHandler> logger)
        : base(unitOfWork, logger)
        {
            _transactionQueryService = transactionQueryService;
            _accountQueryService = accountQueryService;
            _transactionAccessPolicy = transactionAccessPolicy;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionRepository = transactionRepository;
            _transactionPostingPolicy = transactionPostingPolicy;
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

                var accessResult = await _transactionAccessPolicy.EnsureLedgerAccessAsync(userId, existingTransaction.LedgerId);
                if (!accessResult.IsSuccess)
                {
                    return Result.Forbidden(accessResult.Errors.FirstOrDefault() ?? "User does not have access to this ledger");
                }

                if (request.LedgerId.HasValue && request.LedgerId.Value != existingTransaction.LedgerId)
                {
                    return Result.Invalid(new ValidationError
                    {
                        Identifier = nameof(request.LedgerId),
                        ErrorMessage = "Changing ledger for an existing transaction is not supported"
                    });
                }

                if (request.AccountId != existingTransaction.AccountId)
                {
                    return Result.Invalid(new ValidationError
                    {
                        Identifier = nameof(request.AccountId),
                        ErrorMessage = "Changing account for an existing transaction is not supported"
                    });
                }

                var accountDto = await _accountQueryService.GetAccountCurrencyDetailsAsync(existingTransaction.AccountId)
                    .ForUpdate()
                    .QueryFirstOrDefaultAsync();

                if (accountDto is null || accountDto.UserId != userId)
                {
                    return Result.NotFound("Account not found or does not belong to the user");
                }

                int centsAmount = MoneyConverter.ConvertToCents(request.Amount, accountDto.CurrencyFractionalUnitFactor);
                var transactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(request.TransactionTypeId);
                var existingTransactionType = await _transactionTypeQueryService.GetTransactionTypeAsync(existingTransaction.TransactionTypeId);

                if (transactionType is null || existingTransactionType is null)
                {
                    return Result.NotFound("Transaction type not found");
                }

                var newTransaction = new Transaction(
                    existingTransaction.AccountId, existingTransaction.LedgerId, request.TransactionTypeId,
                    request.CategoryId, accountDto.CurrencyId, centsAmount,
                    request.Date, request.Note, request.BudgetId, userId, request.BudgetCategoryId);

                var tranResult = await _transactionRepository.UpdateTransactionAsync(request.TransactionId, newTransaction);
                if (!tranResult)
                {
                    return Result.Error("Unexpected error during updating transaction");
                }

                var newSignedAmount = TransactionHelper.AdjustCentsSign(centsAmount, transactionType.Title);

                var budgetImpactResult = await _transactionPostingPolicy.ApplyBudgetImpactForUpdateAsync(existingTransaction.LedgerId, request, newSignedAmount);
                if (!budgetImpactResult.IsSuccess)
                {
                    return budgetImpactResult.Status switch
                    {
                        ResultStatus.NotFound => Result.NotFound(budgetImpactResult.Errors.FirstOrDefault() ?? "Budget Category not found"),
                        _ => Result.Error(budgetImpactResult.Errors.FirstOrDefault() ?? "Unexpected error during updating budget category")
                    };
                }

                var existingSignedAmount = TransactionHelper.AdjustCentsSign(existingTransaction.Amount, existingTransactionType.Title);

                var accountImpactResult = await _transactionPostingPolicy.ApplyAccountImpactForUpdateAsync(
                    accountDto,
                    existingSignedAmount,
                    newSignedAmount);
                if (!accountImpactResult.IsSuccess)
                {
                    return Result.Error(accountImpactResult.Errors.FirstOrDefault() ?? "Unexpected error during updating account");
                }

                return Result.Success(true);
            }, "Error updating transaction");
        }

    }
}
