using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Responses.Categories;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<Guid>> CreateLedgerCategoryAsync(CreateLedgerCategoryRequest request);
        Task<Result<IEnumerable<GetCategoriesResponse>>> GetCategoriesAsync(Guid ledgerId);
        Task<Result<bool>> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<Result<bool>> DeleteCategoryAsync(DeleteCategoryRequest request);
    }
}
