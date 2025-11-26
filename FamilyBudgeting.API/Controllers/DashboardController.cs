using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] string ledgerId)
        {
            var userId = GetUserIdFromToken();
            if (!Guid.TryParse(ledgerId, out var ledgerGuid))
            {
                return BadRequest("Invalid ledgerId");
            }
            return (await _dashboardService.GetDashboardSummaryAsync(userId, ledgerGuid)).ToActionResult();
        }

        [HttpGet("category-chart")]
        public async Task<IActionResult> GetCategoryChart([FromQuery] string ledgerId, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            var userId = GetUserIdFromToken();
            if (!Guid.TryParse(ledgerId, out var ledgerGuid))
            {
                return BadRequest("Invalid ledgerId");
            }
            if (!DateOnly.TryParse(startDate, out var start))
            {
                return BadRequest("Invalid startDate");
            }
            if (!DateOnly.TryParse(endDate, out var end))
            {
                return BadRequest("Invalid endDate");
            }
            return (await _dashboardService.GetCategoryChartAsync(userId, ledgerGuid, start, end)).ToActionResult();
        }
    }
} 