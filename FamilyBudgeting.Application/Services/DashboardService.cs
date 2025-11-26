using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.DTOs.Responses.Dashboard;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using Ardalis.Result;

namespace FamilyBudgeting.Domain.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardQueryService _dashboardQueryService;
        private readonly ITransactionQueryService _transactionQueryService;

        public DashboardService(IDashboardQueryService dashboardQueryService, ITransactionQueryService transactionQueryService)
        {
            _dashboardQueryService = dashboardQueryService;
            _transactionQueryService = transactionQueryService;
        }

        public async Task<Result<DashBoardSummaryResponse>> GetDashboardSummaryAsync(Guid userId, Guid ledgerId)
        {
            var totalBalance = await _dashboardQueryService.GetTotalBalanceAsync(ledgerId);
            var monthlyIncome = await _dashboardQueryService.GetMonthlyIncomeAsync(ledgerId);
            var monthlyExpense = await _dashboardQueryService.GetMonthlyExpenseAsync(ledgerId);
            var goals = await _dashboardQueryService.GetGoalsAsync(ledgerId);

            return Result.Success(new DashBoardSummaryResponse
            {
                TotalBalance = totalBalance,
                MonthlyIncome = monthlyIncome,
                MonthlyExpense = monthlyExpense,
                Goals = goals
            });
        }

        public async Task<Result<List<CategoryBreakdown>>> GetCategoryChartAsync(Guid userId, Guid ledgerId, DateOnly startDate, DateOnly endDate)
        {
            var stats = await _transactionQueryService.GetLedgerTransactionStatisticsAsync(
                new GetLedgerTransactionStatisticsRequest(userId, ledgerId, startDate, endDate, null)
            );
            var categories = stats.CategoryBreakdowns ?? new List<CategoryBreakdown>();
            return Result.Success(categories);
        }
    }
}
