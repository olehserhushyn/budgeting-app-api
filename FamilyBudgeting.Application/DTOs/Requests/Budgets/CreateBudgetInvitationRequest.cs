namespace FamilyBudgeting.Domain.DTOs.Requests.Budgets
{
    public class CreateBudgetInvitationRequest
    {
        public Guid BudgetId { get; set; }
        public string InvitedEmail { get; set; } = string.Empty;
        public Guid InvitedRoleId { get; set; }
    }
} 