using Ardalis.Result;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Domain.Services
{
    public class TransactionAccessPolicy : ITransactionAccessPolicy
    {
        private readonly ILedgerQueryService _ledgerQueryService;
        private readonly IUserLedgerQueryService _userLedgerQueryService;

        public TransactionAccessPolicy(
            ILedgerQueryService ledgerQueryService,
            IUserLedgerQueryService userLedgerQueryService)
        {
            _ledgerQueryService = ledgerQueryService;
            _userLedgerQueryService = userLedgerQueryService;
        }

        public async Task<Result<Guid>> ResolveLedgerAsync(Guid userId, Guid? ledgerId)
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

        public async Task<Result> EnsureLedgerAccessAsync(Guid userId, Guid ledgerId)
        {
            bool hasAccess = await _userLedgerQueryService.CheckUserLedgerAccessAsync(userId, ledgerId);
            if (!hasAccess)
            {
                return Result.Forbidden("User does not have access to this ledger");
            }

            return Result.Success();
        }
    }
}
