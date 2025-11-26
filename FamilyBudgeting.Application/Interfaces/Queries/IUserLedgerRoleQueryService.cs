using FamilyBudgeting.Domain.DTOs.Models.UserLedgerRoles;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IUserLedgerRoleQueryService
    {
        Task<IEnumerable<UserLedgerRoleDto>> GetUserLedgerRolesAsync();
        Task<UserLedgerRoleDto?> GetUserLedgerRoleByTitleAsync(string title);
    }
}
