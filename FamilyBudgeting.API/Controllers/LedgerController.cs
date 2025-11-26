using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Ledgers;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class LedgerController : BaseController
    {
        private readonly IUserLedgerRoleService _ledgerRoleService;
        private readonly ILedgerService _ledgerService;

        public LedgerController(IUserLedgerRoleService ledgerRoleService, ILedgerService ledgerService)
        {
            _ledgerRoleService = ledgerRoleService;
            _ledgerService = ledgerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateLedger(CreateLedgerRequest request)
        {
            Guid userId = GetUserIdFromToken();

            var result = await _ledgerRoleService.GetUserLedgerRoleByTitleAsync(UserLedgerRoles.Owner);

            if (!result.IsSuccess)
            {
                return result.ToActionResult();
            }

            return (await _ledgerService.CreateLedgerAsync(request, userId, result.Value.Id)).ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetLedgers()
        {
            Guid userId = GetUserIdFromToken();
            return (await _ledgerService.GetLedgersFromUserAsync(userId)).ToActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLedger([FromBody] UpdateLedgerRequest request)
        {
            return (await _ledgerService.UpdateLedgerAsync(request)).ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteLedger([FromBody] DeleteLedgerRequest request)
        {
            return (await _ledgerService.DeleteLedgerAsync(request)).ToActionResult();
        }
    }
}
