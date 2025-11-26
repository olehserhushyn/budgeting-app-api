using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.UserLedgerRoles;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IUserLedgerRoleService
    {
        Task<Result<IEnumerable<UserLedgerRoleDto>>> GetUserLedgerRolesAsync();
        Task<Result<UserLedgerRoleDto>> GetUserLedgerRoleByTitleAsync(string title);
    }
}
