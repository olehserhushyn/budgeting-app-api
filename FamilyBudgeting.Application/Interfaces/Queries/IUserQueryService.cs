using FamilyBudgeting.Domain.DTOs.Models.Users;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IUserQueryService
    {
        Task<UserDto?> GetUserByEmailAsync(string email);
    }
}
