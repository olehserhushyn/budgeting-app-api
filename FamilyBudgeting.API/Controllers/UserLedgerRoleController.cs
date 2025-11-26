using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class UserLedgerRoleController : BaseController
    {
        private readonly IUserLedgerRoleService _userLedgerRoleService;

        public UserLedgerRoleController(IUserLedgerRoleService userLedgerRoleService)
        {
            _userLedgerRoleService = userLedgerRoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserLedgerRoles()
        {
            return (await _userLedgerRoleService.GetUserLedgerRolesAsync()).ToActionResult();
        }
    }
} 