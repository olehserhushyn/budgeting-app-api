using FamilyBudgeting.Application.DTOs.Models.Analytics;

namespace FamilyBudgeting.Domain.Interfaces.Queries
{
    public interface IAnalyticsQueryService
    {
        Task<bool> VerifyUserLedgerAccessAsync(Guid userId, Guid ledgerId);
        Task<IEnumerable<AccountBalanceDataDto>> GetCurrentAccountBalancesAsync(Guid ledgerId);
        Task<IEnumerable<HistoricalAccountDataDto>> GetHistoricalAccountDataAsync(Guid ledgerId, int year);
    }
} 