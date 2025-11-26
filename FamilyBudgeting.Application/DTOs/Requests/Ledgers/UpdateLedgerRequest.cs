namespace FamilyBudgeting.Domain.DTOs.Requests.Ledgers
{
    public class UpdateLedgerRequest
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
    }
} 