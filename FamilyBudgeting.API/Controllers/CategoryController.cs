using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IAccessService _accessService;
        private readonly IBudgetCategoryService _budgetCategoryService;

        public CategoryController(ICategoryService categoryService, IAccessService accessService,
            IBudgetCategoryService budgetCategoryService)
        {
            _categoryService = categoryService;
            _accessService = accessService;
            _budgetCategoryService = budgetCategoryService;
        }

        [HttpPost("ledger-category")]
        public async Task<IActionResult> CreateLedgerCategory([FromBody] CreateLedgerCategoryRequest request)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToLedgerAsync(userId, request.LedgerId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _categoryService.CreateLedgerCategoryAsync(request)).ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] GetCategoriesRequest request)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToLedgerAsync(userId, request.ledgerId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _categoryService.GetCategoriesAsync(request.ledgerId)).ToActionResult();
        }

        [HttpPatch("category")]
        public async Task<IActionResult> UpdateCategory([FromQuery] Guid id, [FromBody] UpdateCategoryRequest request)
        {
            return (await _categoryService.UpdateCategoryAsync(request with { Id = id })).ToActionResult();
        }

        [HttpDelete("category")]
        public async Task<IActionResult> DeleteCategory([FromQuery] Guid id)
        {
            return (await _categoryService.DeleteCategoryAsync(new DeleteCategoryRequest(id))).ToActionResult();
        }
    }
}
