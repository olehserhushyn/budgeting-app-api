using FamilyBudgeting.Domain.DTOs.Models.UserBudgets;

namespace FamilyBudgeting.Domain.DTOs.Responses.UserBudgets
{
    public class GetBudgetUsersResponse
    {
        public List<UserBudgetDetailsDto> BudgetUsers { get; set; } = new List<UserBudgetDetailsDto>();
        public List<UserBudgetDetailsDto> LedgerBudgetUsers { get; set; } = new List<UserBudgetDetailsDto>();
    }
}
