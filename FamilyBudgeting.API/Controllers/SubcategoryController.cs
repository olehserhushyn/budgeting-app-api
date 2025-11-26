using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Subcategories;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class SubcategoryController : BaseController
    {
        private readonly ISubcategoryService _subcategoryService;

        public SubcategoryController(ISubcategoryService subcategoryService)
        {
            _subcategoryService = subcategoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubcategory(CreateSubcategoryRequest request)
        {
            Guid userId = GetUserIdFromToken();

            return (await _subcategoryService.CreateTransactionAsync(request)).ToActionResult();
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateSubcategory([FromQuery] Guid id, [FromBody] UpdateSubcategoryRequest request)
        {
            return (await _subcategoryService.UpdateSubcategoryAsync(request with { Id = id })).ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSubcategory([FromQuery] Guid id)
        {
            return (await _subcategoryService.DeleteSubcategoryAsync(new DeleteSubcategoryRequest(id))).ToActionResult();
        }
    }
}
