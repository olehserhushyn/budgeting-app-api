using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.DTOs.Responses.Categories;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.Categories;

namespace FamilyBudgeting.Domain.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISubcategoryQueryService _subcategoryQueryService;
        private readonly ICategoryQueryService _categoryQueryService;

        public CategoryService(ICategoryRepository categoryRepository, ISubcategoryQueryService subcategoryQueryService, 
            ICategoryQueryService categoryQueryService)
        {
            _categoryRepository = categoryRepository;
            _subcategoryQueryService = subcategoryQueryService;
            _categoryQueryService = categoryQueryService;
        }

        public async Task<Result<IEnumerable<GetCategoriesResponse>>> GetCategoriesAsync(Guid ledgerId)
        {
            var categories = await _categoryQueryService.GetCategoriesDetailsAsync(ledgerId);
            return Result.Success(categories);
        }

        public async Task<Result<Guid>> CreateLedgerCategoryAsync(CreateLedgerCategoryRequest request)
        {
            var category = new Category(request.Title, request.LedgerId, request.TransactionTypeId);

            Guid categoryId = await _categoryRepository.CreateCategoryAsync(category);

            return Result.Success(categoryId);
        }

        public async Task<Result<bool>> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            var categoryDto = await _categoryQueryService.GetCategoryAsync(request.Id);
            if (categoryDto is null)
            {
                return Result<bool>.NotFound($"Category not found: {request.Id}");
            }

            var category = new Category(categoryDto.Title, categoryDto.LedgerId, categoryDto.TransactionTypeId) { Id = categoryDto.Id };
            category.UpdateDetails(request.Title, request.LedgerId, request.TransactionTypeId);

            bool updated = await _categoryRepository.UpdateCategoryAsync(request.Id, category);
            return Result.Success(updated);
        }

        public async Task<Result<bool>> DeleteCategoryAsync(DeleteCategoryRequest request)
        {
            var categoryDto = await _categoryQueryService.GetCategoryAsync(request.Id);
            if (categoryDto is null)
            {
                return Result<bool>.NotFound($"Category not found: {request.Id}");
            }

            var category = new Category(categoryDto.Title, categoryDto.LedgerId, categoryDto.TransactionTypeId) { Id = categoryDto.Id };
            category.Delete();

            var subIds = await _subcategoryQueryService.GetSubcategoryIdsFromCategoryAsync(request.Id);
            bool deleted = await _categoryRepository.DeleteCategoryAsync(request.Id, subIds ?? new List<Guid>());

            if (deleted)
            {
                return Result.Success(true);
            }

            return Result.Error($"Unable to delete category with Id: {request.Id}");
        }
    }
}
