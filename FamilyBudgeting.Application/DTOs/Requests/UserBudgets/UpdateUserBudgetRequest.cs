using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Requests.UserLedgers
{
    public record UpdateUserBudgetRequest(
        [Required]
        Guid UserId,
        [Required]
        Guid RoleId,
        [Required]
        Guid BudgetId
        );
}
