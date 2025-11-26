using Ardalis.Result;
using FamilyBudgeting.Application.DTOs.Models.Analytics;
using FamilyBudgeting.Application.DTOs.Requests.Analytics;
using FamilyBudgeting.Application.DTOs.Responses.Analytics;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;

namespace FamilyBudgeting.Application.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsQueryService _analyticsQueryService;
        private readonly IAccessQueryService _accessQueryService;

        public AnalyticsService(IAnalyticsQueryService analyticsQueryService, IAccessQueryService accessQueryService)
        {
            _analyticsQueryService = analyticsQueryService;
            _accessQueryService = accessQueryService;
        }

        public async Task<Result<GetAccountSummaryResponse>> GetAccountSummaryAsync(Guid userId, GetAccountSummaryRequest request)
        {
            // Verify user access
            var hasAccess = await _accessQueryService.UserHasAccessToLedgerAsync(userId, request.LedgerId);
            if (!hasAccess)
            {
                return Result<GetAccountSummaryResponse>.Unauthorized("You don't have access to this ledger");
            }

            // Get raw data from query service
            var currentBalances = await _analyticsQueryService.GetCurrentAccountBalancesAsync(request.LedgerId);
            var historicalData = await _analyticsQueryService.GetHistoricalAccountDataAsync(request.LedgerId, request.Year);

            // Business logic: Calculate total assets
            var totalAssets = currentBalances.Sum(b => b.Balance);

            // Business logic: Build balance distribution by account type
            var balanceDistribution = currentBalances
                .GroupBy(b => b.AccountTypeName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(b => b.Balance)
                );

            // Business logic: Build performance by account type
            var performanceByType = BuildPerformanceByType(currentBalances, historicalData, totalAssets);

            var response = new GetAccountSummaryResponse
            {
                TotalAssets = totalAssets,
                BalanceDistribution = balanceDistribution,
                PerformanceByType = performanceByType
            };

            return Result<GetAccountSummaryResponse>.Success(response);
        }

        private Dictionary<string, AccountTypePerformance> BuildPerformanceByType(
            IEnumerable<AccountBalanceDataDto> currentBalances, 
            IEnumerable<HistoricalAccountDataDto> historicalData, 
            decimal totalAssets)
        {
            var performanceByType = new Dictionary<string, AccountTypePerformance>();

            // Group current balances by account type
            var currentByType = currentBalances.GroupBy(b => b.AccountTypeName);

            foreach (var typeGroup in currentByType)
            {
                var accountTypeName = typeGroup.Key;
                var typeTotalAmount = typeGroup.Sum(b => b.Balance);
                var percentageOfTotal = totalAssets > 0 ? (typeTotalAmount / totalAssets) * 100 : 0;

                var accounts = new Dictionary<string, AccountPerformance>();

                foreach (var account in typeGroup)
                {
                    var historicalAccount = historicalData.FirstOrDefault(h => h.AccountId == account.AccountId);
                    var growthPercentage = CalculateGrowthPercentage(account.Balance, historicalAccount?.NetChange ?? 0);

                    accounts[account.AccountTitle] = new AccountPerformance
                    {
                        Amount = account.Balance,
                        GrowthPercentage = growthPercentage
                    };
                }

                performanceByType[accountTypeName] = new AccountTypePerformance
                {
                    TotalAmount = typeTotalAmount,
                    PercentageOfTotal = percentageOfTotal,
                    Accounts = accounts
                };
            }

            return performanceByType;
        }

        private decimal CalculateGrowthPercentage(decimal currentAmount, decimal netChange)
        {
            if (currentAmount == 0 && netChange == 0)
                return 0;

            var previousAmount = currentAmount - netChange;
            if (previousAmount == 0)
                return netChange > 0 ? 100 : (netChange < 0 ? -100 : 0);

            return (netChange / previousAmount) * 100;
        }
    }
} 