namespace FamilyBudgeting.Domain.DTOs.Models.UserLedgers
{
    public class UserLedgerDto
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public string RoleTitle { get; set; }
        public Guid LedgerId { get; private set; }
    }
}
