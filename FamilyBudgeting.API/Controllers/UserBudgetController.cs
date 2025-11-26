using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.UserBudgets;
using FamilyBudgeting.Domain.DTOs.Requests.UserLedgers;
using FamilyBudgeting.Domain.Services;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Budgets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class UserBudgetController : BaseController
    {
        private readonly IAccessService _accessService;
        private readonly IUserBudgetService _userBudgetService;

        public UserBudgetController(IAccessService accessService, IUserBudgetService userBudgetService)
        {
            _accessService = accessService;
            _userBudgetService = userBudgetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgetUsers([FromQuery] Guid budgetId)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToBudgetAsync(userId, budgetId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _userBudgetService.GetBudgetUsersAsync(userId, budgetId)).ToActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserBudget(UpdateUserBudgetRequest request)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToBudgetAsync(userId, request.BudgetId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _userBudgetService.UpdateUserBudgetAsync(userId, request)).ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteUserBudgetRequest request)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToBudgetAsync(userId, request.BudgetId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _userBudgetService.DeleteUserBudgetAsync(userId, request)).ToActionResult();
        }
    }
}
