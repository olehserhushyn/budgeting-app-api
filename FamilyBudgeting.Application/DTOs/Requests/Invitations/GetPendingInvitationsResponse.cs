using FamilyBudgeting.Domain.DTOs.Models.Invitations;

namespace FamilyBudgeting.Domain.DTOs.Requests.Invitations
{
    public class GetPendingInvitationsResponse
    {
        public List<InvitationDto> LedgerInvitations { get; set; } = new List<InvitationDto>();
        public List<InvitationDto> BudgetInvitations { get; set; } = new List<InvitationDto>();
    }
}
