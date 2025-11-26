using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Ledgers;
using FamilyBudgeting.Domain.DTOs.Requests.Ledgers;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ILedgerService
    {
        Task<Result<Guid>> CreateLedgerAsync(CreateLedgerRequest request, Guid userId, Guid roleId);
        Task<Result<IEnumerable<LedgerDto>>> GetLedgersFromUserAsync(Guid userId);
        Task<Result<bool>> UpdateLedgerAsync(UpdateLedgerRequest request);
        Task<Result<bool>> DeleteLedgerAsync(DeleteLedgerRequest request);
    }
}
