namespace FamilyBudgeting.Domain.DTOs.Models.UserLedgers
{
    public class UserLedgerDetailsDto
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public Guid RoleId { get; private set; }
        public string RoleTitle { get; set; }
        public Guid LedgerId { get; private set; }
    }
}
