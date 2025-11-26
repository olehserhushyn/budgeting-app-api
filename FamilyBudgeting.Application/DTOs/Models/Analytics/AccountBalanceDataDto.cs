namespace FamilyBudgeting.Application.DTOs.Models.Analytics
{
    public class AccountBalanceDataDto
    {
        public Guid AccountId { get; set; }
        public string AccountTitle { get; set; }
        public decimal Balance { get; set; }
        public string AccountTypeName { get; set; }
    }
}
