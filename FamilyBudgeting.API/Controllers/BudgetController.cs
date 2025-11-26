using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Budgets;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class BudgetController : BaseController
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget(CreateBudgetRequest request)
        {
            Guid userId = GetUserIdFromToken();

            return (await _budgetService.CreateBudgetAsync(request)).ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetLedgerBudgets([FromQuery] Guid ledgerId)
        {
            return (await _budgetService.GetBudgetsFromLedgerAsync(ledgerId)).ToActionResult();
        }

        [HttpGet("{budgetId}")]
        public async Task<IActionResult> GetBudgetDetails(Guid budgetId)
        {
            return (await _budgetService.GetBudetDetailsAsync(budgetId)).ToActionResult();
        }

        [HttpPut("{budgetId}")]
        public async Task<IActionResult> UpdateBudget(Guid budgetId, [FromBody] UpdateBudgetRequest request)
        {
            return (await _budgetService.UpdateBudgetAsync(budgetId, request)).ToActionResult();
        }

        [HttpDelete("{budgetId}")]
        public async Task<IActionResult> DeleteBudget(Guid budgetId)
        {
            return (await _budgetService.DeleteBudgetAsync(budgetId)).ToActionResult();
        }
    }
}
