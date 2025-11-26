using Ardalis.Result;
using FamilyBudgeting.Application.DTOs.Requests.Analytics;
using FamilyBudgeting.Application.DTOs.Responses.Analytics;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<Result<GetAccountSummaryResponse>> GetAccountSummaryAsync(Guid userId, GetAccountSummaryRequest request);
    }
} 