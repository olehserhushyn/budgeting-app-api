using FamilyBudgeting.Domain.Data.ValueObjects;

namespace FamilyBudgeting.Domain.DTOs.Models.Invitations
{
    public class InvitationDto
    {
        public Guid Id { get; set; }
        public string InvitedEmail { get; set; }
        public Guid InvitedRoleId { get; set; }
        public Guid InviterUserId { get; set; }
        public DestinationType DestinationType { get; set; }
        public Guid DestinationId { get; set; }
        public InvitationStatus Status { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
