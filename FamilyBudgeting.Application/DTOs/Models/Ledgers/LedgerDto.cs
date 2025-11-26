namespace FamilyBudgeting.Domain.DTOs.Models.Ledgers
{
    public class LedgerDto
    {
        public string? Title { get; set; }
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
