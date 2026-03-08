using Ardalis.Result;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Utilities;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionCreateHandler : TransactionCommandHandlerBase, ITransactionCreateHandler
    {
        private readonly ITransactionAccessPolicy _transactionAccessPolicy;
        private readonly IAccountQueryService _accountQueryService;
        private readonly ITransactionTypeQueryService _transactionTypeQueryService;
        private readonly ITransactionCategoryResolutionPolicy _transactionCategoryResolutionPolicy;
        private readonly ITransactionPostingPolicy _transactionPostingPolicy;
        private readonly ITransactionRepository _transactionRepository;

        public TransactionCreateHandler(
            ITransactionAccessPolicy transactionAccessPolicy,
            IAccountQueryService accountQueryService,
            ITransactionTypeQueryService transactionTypeQueryService,
            ITransactionCategoryResolutionPolicy transactionCategoryResolutionPolicy,
            ITransactionPostingPolicy transactionPostingPolicy,
            IUnitOfWork unitOfWork,
            Microsoft.Extensions.Logging.ILogger<TransactionCreateHandler> logger,
            ITransactionRepository transactionRepository)
        : base(unitOfWork, logger)
        {
            _transactionAccessPolicy = transactionAccessPolicy;
            _accountQueryService = accountQueryService;
            _transactionTypeQueryService = transactionTypeQueryService;
            _transactionCategoryResolutionPolicy = transactionCategoryResolutionPolicy;
            _transactionPostingPolicy = transactionPostingPolicy;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<Guid>> HandleAsync(Guid userId, CreateTransactionRequest request)
        {
            return await ExecuteInTransactionAsync(async () =>
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

                var categoryResolutionResult = await _transactionCategoryResolutionPolicy.ResolveForCreateAsync(userId, existingLedgerId, request);
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

                var budgetImpactResult = await _transactionPostingPolicy.ApplyBudgetImpactForCreateAsync(existingLedgerId, request, centsAmountWithSign);
                if (!budgetImpactResult.IsSuccess)
                {
                    return budgetImpactResult.Status switch
                    {
                        ResultStatus.NotFound => Result.NotFound(budgetImpactResult.Errors.FirstOrDefault() ?? "Budget Category not found"),
                        _ => Result.Error(budgetImpactResult.Errors.FirstOrDefault() ?? "Unexpected error during updating budget category")
                    };
                }

                var accountImpactResult = await _transactionPostingPolicy.ApplyAccountImpactForCreateAsync(accountDto, centsAmountWithSign);
                if (!accountImpactResult.IsSuccess)
                {
                    return Result.Error(accountImpactResult.Errors.FirstOrDefault() ?? "Unexpected error during updating account");
                }

                return Result.Success(trId);
            }, "Error creating transaction");
        }

    }
}
