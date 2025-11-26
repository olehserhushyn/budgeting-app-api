using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Requests.UserBudgets
{
    public record DeleteUserBudgetRequest(
        [Required]
        Guid UserId,
        [Required]
        Guid BudgetId);
}
