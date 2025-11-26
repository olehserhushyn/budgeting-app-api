namespace FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories
{
    public class ImportBudgetCategoriesResponse
    {
        public bool Success { get; set; }
        public List<Guid>? ImportedCategoryIds { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 