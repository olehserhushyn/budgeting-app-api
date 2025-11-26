using FamilyBudgeting.Domain.DTOs.Responses.Dashboard;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IDashboardQueryService
    {
        Task<DashBoardSummaryResponse_TotalBalance> GetTotalBalanceAsync(Guid ledgerId);
        Task<DashBoardSummaryResponse_MonthlyFlow> GetMonthlyIncomeAsync(Guid ledgerId);
        Task<DashBoardSummaryResponse_MonthlyFlow> GetMonthlyExpenseAsync(Guid ledgerId);
        Task<DashBoardSummaryResponse_Goal> GetGoalsAsync(Guid ledgerId);
    }
}
