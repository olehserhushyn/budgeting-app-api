using FamilyBudgeting.Domain.DTOs.Models.UserLedgers;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IUserLedgerQueryService
    {
        Task<bool> CheckUserLedgerAccessAsync(Guid userId, Guid ledgerId);
        Task<UserLedgerDto?> GetUserLedgerDtoByIdAsync(Guid ledgerId, Guid id);
        Task<UserLedgerDto?> GetUserLedgerDtoByUserIdAsync(Guid ledgerId, Guid userId);
        Task<IEnumerable<UserLedgerDetailsDto>> GetUserLedgerDtoByLedgerIdAsync(Guid ledgerId);
        Task<bool> IsUserInLedgerRoleAsync(Guid userId, Guid ledgerId, params string[] roleTitles);
    }
}
