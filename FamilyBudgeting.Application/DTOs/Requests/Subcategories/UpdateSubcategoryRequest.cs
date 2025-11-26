namespace FamilyBudgeting.Domain.DTOs.Requests.Subcategories
{
    public record UpdateSubcategoryRequest(Guid Id, string Title, Guid CategoryId);
}
