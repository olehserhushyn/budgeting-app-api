namespace FamilyBudgeting.Domain.DTOs.Requests.Subcategories
{
    public record CreateSubcategoryRequest(string Title, Guid CategoryId);
}
