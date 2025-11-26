namespace FamilyBudgeting.Domain.DTOs.Requests.Ledgers
{
    public class CreateLedgerInvitationRequest
    {
        public Guid LedgerId { get; set; }
        public string InvitedEmail { get; set; } = string.Empty;
        public Guid InvitedRoleId { get; set; }
    }
} 