using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Ledgers;
using FamilyBudgeting.Domain.DTOs.Requests.Budgets;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    [Route("[controller]")]
    public class InvitationController : BaseController
    {
        private readonly IInvitationService _invitationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvitationController(IInvitationService invitationService, UserManager<ApplicationUser> userManager)
        {
            _invitationService = invitationService;
            _userManager = userManager;
        }

        [HttpPost("ledger")]
        public async Task<IActionResult> Create([FromBody] CreateLedgerInvitationRequest request)
        {
            var userId = GetUserIdFromToken();
            return (await _invitationService.CreateLedgerInvitationAsync(request, userId)).ToActionResult();
        }

        [HttpPost("budget")]
        public async Task<IActionResult> CreateBudgetInvitation([FromBody] CreateBudgetInvitationRequest request)
        {
            var userId = GetUserIdFromToken();
            return (await _invitationService.CreateBudgetInvitationAsync(request, userId)).ToActionResult();
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetByToken([FromQuery] string token)
        {
            return (await _invitationService.GetByTokenAsync(token)).ToActionResult();
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var userId = GetUserIdFromToken();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return NotFound("Current user not found");
            }

            return (await _invitationService.GetPendingByEmailAsync(user.Email)).ToActionResult();
        }

        [HttpPut("accept/{token}")]
        public async Task<IActionResult> Accept([FromQuery] string token)
        {
            var userId = GetUserIdFromToken();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return NotFound("Current user not found");
            }

            var result = await _invitationService.AcceptInvitationAsync(token, userId, user.Email);
            return result.ToActionResult();
        }

        [HttpPut("decline/{token}")]
        public async Task<IActionResult> Decline([FromQuery] string token)
        {
            var userId = GetUserIdFromToken();
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return NotFound("Current user not found");
            }

            var result = await _invitationService.DeclineInvitationAsync(token, user.Email);
            return result.ToActionResult();
        }
    }
} 