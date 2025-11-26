using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Responses.Dashboard;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;

namespace FamilyBudgeting.Domain.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<Result<DashBoardSummaryResponse>> GetDashboardSummaryAsync(Guid userId, Guid ledgerId);
        Task<Result<List<CategoryBreakdown>>> GetCategoryChartAsync(Guid userId, Guid ledgerId, DateOnly startDate, DateOnly endDate);
    }
}
