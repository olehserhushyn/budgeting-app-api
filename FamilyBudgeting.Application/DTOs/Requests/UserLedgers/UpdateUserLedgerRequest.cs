using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Requests.UserLedgers
{
    public record UpdateUserLedgerRequest(
        [Required]
        Guid UserId,
        [Required]
        Guid RoleId,
        [Required]
        Guid LedgerId
        );
}
