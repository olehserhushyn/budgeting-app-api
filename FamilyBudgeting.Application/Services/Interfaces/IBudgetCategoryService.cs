using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.DTOs.Responses.BudgetCategories;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IBudgetCategoryService
    {
        Task<Result<BudgetCategoryDto>> GetBudgetCategoryAsync(Guid userId, Guid id);
        Task<Result> UpdateBudgetCategoryAsync(Guid id, UpdateBudgetCategoryRequest request);
        Task<Result<Guid>> CreateBudgetCategoryAsync(Guid userId, CreateBudgetCategoryRequest request);
        Task<Result<IEnumerable<BudgetCategoriesDetailedResponse>>> GetBudgetCategoriesAsync(Guid userId, Guid budgetId);
        Task<Result> DeleteBudgetCategoryAsync(Guid id);
        Task<Result<ImportBudgetCategoriesResponse>> ImportBudgetCategoriesAsync(Guid userId, ImportBudgetCategoriesRequest request);
    }
}
