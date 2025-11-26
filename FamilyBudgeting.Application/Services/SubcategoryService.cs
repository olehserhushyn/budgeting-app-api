using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Subcategories;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.Subcategories;

namespace FamilyBudgeting.Domain.Services
{
    public class SubcategoryService : ISubcategoryService
    {
        private readonly ISubcategoryRepository _subcategoryRepository;
        private readonly ISubcategoryQueryService _subcategoryQueryService;

        public SubcategoryService(ISubcategoryRepository subcategoryRepository, ISubcategoryQueryService subcategoryQueryService)
        {
            _subcategoryRepository = subcategoryRepository;
            _subcategoryQueryService = subcategoryQueryService;
        }

        public async Task<Result<Guid>> CreateTransactionAsync(CreateSubcategoryRequest request)
        {
            var subcategory = new Subcategory(request.Title, request.CategoryId);

            Guid subId = await _subcategoryRepository.CreateSubcategoryAsync(subcategory);

            return Result.Success(subId);
        }

        public async Task<Result<bool>> UpdateSubcategoryAsync(UpdateSubcategoryRequest request)
        {
            var subcategoryDto = await _subcategoryQueryService.GetSubcategoryAsync(request.Id);
            if (subcategoryDto is null)
                return Result<bool>.NotFound($"Subcategory not found: {request.Id}");
            var subcategory = new Subcategory(subcategoryDto.Title, subcategoryDto.CategoryId) { Id = subcategoryDto.Id };
            subcategory.UpdateDetails(request.Title, request.CategoryId);
            bool updated = await _subcategoryRepository.UpdateSubcategoryAsync(request.Id, subcategory);
            return Result.Success(updated);
        }

        public async Task<Result<bool>> DeleteSubcategoryAsync(DeleteSubcategoryRequest request)
        {
            var subcategoryDto = await _subcategoryQueryService.GetSubcategoryAsync(request.Id);
            if (subcategoryDto is null)
                return Result<bool>.NotFound($"Subcategory not found: {request.Id}");
            var subcategory = new Subcategory(subcategoryDto.Title, subcategoryDto.CategoryId) { Id = subcategoryDto.Id };
            subcategory.Delete();
            bool deleted = await _subcategoryRepository.DeleteSubcategoryAsync(request.Id);
            return Result.Success(deleted);
        }
    }
}
