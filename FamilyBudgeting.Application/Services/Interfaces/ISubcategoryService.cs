using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Subcategories;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface ISubcategoryService
    {
        Task<Result<Guid>> CreateTransactionAsync(CreateSubcategoryRequest request);
        Task<Result<bool>> UpdateSubcategoryAsync(UpdateSubcategoryRequest request);
        Task<Result<bool>> DeleteSubcategoryAsync(DeleteSubcategoryRequest request);
    }
}
