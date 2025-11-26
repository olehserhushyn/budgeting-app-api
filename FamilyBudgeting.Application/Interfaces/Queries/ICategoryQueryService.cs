using FamilyBudgeting.Domain.DTOs.Models.Categories;
using FamilyBudgeting.Domain.DTOs.Responses.Categories;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface ICategoryQueryService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync(Guid ledgerId);
        Task<IEnumerable<GetCategoriesResponse>> GetCategoriesDetailsAsync(Guid ledgerId);
        Task<CategoryDto?> GetCategoryAsync(Guid id);
        Task<CategoryDto?> GetCategoryAsync(string title);
    }
}
