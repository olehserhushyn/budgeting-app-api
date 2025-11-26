using FamilyBudgeting.Domain.Data.ValueObjects;
using FamilyBudgeting.Domain.Exceptions;

namespace FamilyBudgeting.Domain.Data.Invitations
{
    public class Invitation : BaseEntity
    {
        public string InvitedEmail { get; private set; } = string.Empty;
        public Guid InvitedRoleId { get; private set; }
        public Guid InviterUserId { get; private set; }

        // Polymorphic destination reference
        public DestinationType DestinationType { get; private set; }
        public Guid DestinationId { get; private set; } // Can point to either a Ledger or Budget

        public InvitationStatus Status { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }

        public Invitation(string invitedEmail, Guid invitedRoleId, Guid inviterUserId,
            DestinationType destinationType, Guid destinationId, InvitationStatus status,
            string token, DateTime expiresAt)
        {
            InvitedEmail = invitedEmail.ToLower();
            InvitedRoleId = invitedRoleId;
            InviterUserId = inviterUserId;
            DestinationType = destinationType;
            DestinationId = destinationId;
            Status = status;
            Token = token;
            ExpiresAt = expiresAt;
        }

        public void AcceptInvite(string invitedEmail)
        {
            if (string.IsNullOrEmpty(invitedEmail))
            {
                throw new DomainValidationException("Invited Email is null or empty");
            }

            if (invitedEmail != this.InvitedEmail)
            {
                throw new DomainValidationException("You cannot accept this invite");
            }

            if (this.ExpiresAt > DateTime.UtcNow || this.Status != InvitationStatus.Pending)
            {
                throw new DomainValidationException("Invitation expired");
            }

            this.Status = InvitationStatus.Accepted;
            this.UpdatedAt = DateTime.UtcNow;
        }

        public void DeclineInvite(string invitedEmail)
        {
            if (string.IsNullOrEmpty(invitedEmail))
            {
                throw new DomainValidationException("Invited Email is null or empty");
            }

            if (invitedEmail != this.InvitedEmail)
            {
                throw new DomainValidationException("You cannot accept this invite");
            }

            if (this.ExpiresAt > DateTime.UtcNow || this.Status != InvitationStatus.Pending)
            {
                throw new DomainValidationException("Invitation expired");
            }

            this.Status = InvitationStatus.Declined;
            this.UpdatedAt = DateTime.UtcNow;
        }

        public void ExpireInvite(string invitedEmail)
        {
            if (string.IsNullOrEmpty(invitedEmail))
            {
                throw new DomainValidationException("Invited Email is null or empty");
            }

            if (invitedEmail != this.InvitedEmail)
            {
                throw new DomainValidationException("You cannot accept this invite");
            }

            if (this.Status != InvitationStatus.Pending)
            {
                throw new DomainValidationException("Invitation cannot be changed");
            }

            this.Status = InvitationStatus.Expired;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
} 