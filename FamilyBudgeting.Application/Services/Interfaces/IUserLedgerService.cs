using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.UserLedgers;
using FamilyBudgeting.Domain.DTOs.Requests.UserLedgers;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IUserLedgerService
    {
        Task<Result> CheckUserLedgerAccess(Guid userId, Guid ledgerId);
        Task<Result<IEnumerable<UserLedgerDetailsDto>>> GetLedgerUsersAsync(Guid userId, Guid ledgerId);
        Task<Result> UpdateUserLedgerAsync(Guid userId, UpdateUserLedgerRequest request);
        Task<Result> DeleteUserLedgerAsync(Guid userId, DeleteUserLedgerRequest request);
    }
}
