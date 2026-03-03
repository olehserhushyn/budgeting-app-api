using Ardalis.Result;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ITransactionAccessPolicy
    {
        Task<Result<Guid>> ResolveLedgerAsync(Guid userId, Guid? ledgerId);
        Task<Result> EnsureLedgerAccessAsync(Guid userId, Guid ledgerId);
    }
}
