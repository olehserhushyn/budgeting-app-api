using System;
using FamilyBudgeting.Domain.Data.Ledgers;
using FamilyBudgeting.Domain.Data.ValueObjects;

namespace FamilyBudgeting.Domain.DTOs.Responses.Ledgers
{
    public class LedgerInvitationResponse
    {
        public Guid Id { get; set; }
        public Guid LedgerId { get; set; }
        public string InvitedEmail { get; set; } = string.Empty;
        public Guid InvitedRoleId { get; set; }
        public Guid InviterUserId { get; set; }
        public InvitationStatus Status { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
} 