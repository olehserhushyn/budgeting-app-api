using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.UserLedgers;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class UserLedgerController : BaseController
    {
        private readonly IUserLedgerService _userLedgerService;
        private readonly IAccessService _accessService;

        public UserLedgerController(IUserLedgerService userLedgerService, IAccessService accessService)
        {
            _userLedgerService = userLedgerService;
            _accessService = accessService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLedgerUsers([FromQuery] Guid ledgerId)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToLedgerAsync(userId, ledgerId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _userLedgerService.GetLedgerUsersAsync(userId, ledgerId)).ToActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserLedger(UpdateUserLedgerRequest request)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToLedgerAsync(userId, request.LedgerId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _userLedgerService.UpdateUserLedgerAsync(userId, request)).ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteUserLedgerRequest request)
        {
            Guid userId = GetUserIdFromToken();

            bool hasAccess = await _accessService.UserHasAccessToLedgerAsync(userId, request.LedgerId);

            if (!hasAccess)
            {
                return Forbid();
            }

            return (await _userLedgerService.DeleteUserLedgerAsync(userId, request)).ToActionResult();
        }
    }
}
