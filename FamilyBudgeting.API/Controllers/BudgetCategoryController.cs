using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.BudgetCategories;
using FamilyBudgeting.Domain.DTOs.Requests.Categories;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class BudgetCategoryController : BaseController
    {
        private readonly IBudgetCategoryService _budgetCategoryService;
        private readonly IAccessService _accessService;

        public BudgetCategoryController(IBudgetCategoryService budgetCategoryService, IAccessService accessService)
        {
            _budgetCategoryService = budgetCategoryService;
            _accessService = accessService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudgetCategory([FromBody] CreateBudgetCategoryRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _budgetCategoryService.CreateBudgetCategoryAsync(userId, request)).ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgetCategories([FromQuery] Guid budgetId)
        {
            Guid userId = GetUserIdFromToken();
            bool hasAccess = await _accessService.UserHasAccessToBudgetAsync(userId, budgetId);
            if (!hasAccess)
            {
                return Forbid();
            }
            return (await _budgetCategoryService.GetBudgetCategoriesAsync(userId, budgetId)).ToActionResult();
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateBudgetCategory([FromQuery] Guid id, [FromBody] UpdateBudgetCategoryRequest request)
        {
            Guid userId = GetUserIdFromToken();
            bool hasAccess = await _accessService.UserHasAccessToBudgetCategoryAsync(userId, id);
            if (!hasAccess)
            {
                return Forbid();
            }
            return (await _budgetCategoryService.UpdateBudgetCategoryAsync(id, request)).ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBudgetCategory([FromQuery] Guid id)
        {
            return (await _budgetCategoryService.DeleteBudgetCategoryAsync(id)).ToActionResult();
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportBudgetCategories([FromBody] ImportBudgetCategoriesRequest request)
        {
            Guid userId = GetUserIdFromToken();
            // Optionally: validate user access to both budgets here or in service
            return (await _budgetCategoryService.ImportBudgetCategoriesAsync(userId, request)).ToActionResult();
        }
    }
} 