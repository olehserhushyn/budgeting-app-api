namespace FamilyBudgeting.Domain.DTOs.Models.Accounts
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountTypeId { get; set; }
        public Guid CurrencyId { get; set; }
        public string Title { get; set; }
        public int Balance { get; set; }
    }
}
