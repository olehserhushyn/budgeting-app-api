using FamilyBudgeting.Domain.DTOs.Models.Subcategories;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface ISubcategoryQueryService
    {
        Task<IEnumerable<Guid>> GetSubcategoryIdsFromCategoryAsync(Guid categoryId);
        Task<SubcategoryDto?> GetSubcategoryAsync(Guid id);
    }
}
