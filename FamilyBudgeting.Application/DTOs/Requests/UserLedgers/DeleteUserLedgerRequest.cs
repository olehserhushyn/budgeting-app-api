using System.ComponentModel.DataAnnotations;

namespace FamilyBudgeting.Domain.DTOs.Requests.UserLedgers
{
    public record DeleteUserLedgerRequest(
        [Required]
        Guid UserId,
        [Required]
        Guid LedgerId);
}
