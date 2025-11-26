namespace FamilyBudgeting.Domain.DTOs.Models.Accounts
{
    public class AccountDetailsDto
    {
        public Guid AccountId { get; set; }
        public DateTime AccountCreatedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountTypeId { get; set; }
        public string AccountTitle { get; set; }
        public string AccountTypeName { get; set; }
        public int AccountBalance { get; set; }
    }
}
