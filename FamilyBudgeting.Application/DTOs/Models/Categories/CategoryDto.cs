namespace FamilyBudgeting.Domain.DTOs.Models.Categories
{
    public class CategoryDto
    {
        public Guid Id { get; init; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string Title { get; set; }
        public Guid LedgerId { get; set; }
        public Guid TransactionTypeId { get; set; }
    }
}
