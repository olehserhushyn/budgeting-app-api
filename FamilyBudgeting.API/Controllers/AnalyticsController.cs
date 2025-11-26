using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Application.DTOs.Requests.Analytics;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class AnalyticsController : BaseController
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountSummary([FromQuery] GetAccountSummaryRequest request)
        {
            Guid userId = GetUserIdFromToken();
            return (await _analyticsService.GetAccountSummaryAsync(userId, request)).ToActionResult();
        }
    }
}
