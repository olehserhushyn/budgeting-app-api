using Dapper;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using FamilyBudgeting.Application.DTOs.Models.Analytics;
using FamilyBudgeting.Domain.Constants;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class AnalyticsQueryService : IAnalyticsQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AnalyticsQueryService> _logger;

        public AnalyticsQueryService(IUnitOfWork unitOfWork, ILogger<AnalyticsQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> VerifyUserLedgerAccessAsync(Guid userId, Guid ledgerId)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM ""UserLedger"" 
                WHERE ""UserId"" = @UserId 
                  AND ""LedgerId"" = @LedgerId 
                  AND ""IsDeleted"" = false
            ";

            var qparams = new { UserId = userId, LedgerId = ledgerId };
            _logger.LogQuery(query, qparams);

            var count = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<int>(query, qparams, _unitOfWork.Transaction);
            return count > 0;
        }

        public async Task<IEnumerable<AccountBalanceDataDto>> GetCurrentAccountBalancesAsync(Guid ledgerId)
        {
            string query = @"
                SELECT 
                    a.""Id"" AS AccountId,
                    a.""Title"" AS AccountTitle,
                    a.""Balance"" AS RawBalance,
                    at.""Title"" AS AccountTypeName,
                    c.""FractionalUnitFactor"" AS FractionalUnitFactor
                FROM ""Account"" a
                JOIN ""AccountType"" at ON a.""AccountTypeId"" = at.""Id""
                JOIN ""Currency"" c ON a.""CurrencyId"" = c.""Id""
                JOIN ""UserLedger"" ul ON a.""UserId"" = ul.""UserId""
                WHERE ul.""LedgerId"" = @LedgerId 
                  AND a.""IsDeleted"" = false
                  AND ul.""IsDeleted"" = false
                ORDER BY at.""Title"", a.""Title""
            ";

            var qparams = new { LedgerId = ledgerId };
            _logger.LogQuery(query, qparams);

            var results = await _unitOfWork.Connection.QueryAsync<AccountBalanceDataRawDto>(query, qparams, _unitOfWork.Transaction);
            
            // Convert raw balance to decimal
            return results.Select(r => new AccountBalanceDataDto
            {
                AccountId = r.AccountId,
                AccountTitle = r.AccountTitle,
                AccountTypeName = r.AccountTypeName,
                Balance = (decimal)r.RawBalance / (r.FractionalUnitFactor == 0 ? 1 : r.FractionalUnitFactor)
            });
        }

        public async Task<IEnumerable<HistoricalAccountDataDto>> GetHistoricalAccountDataAsync(Guid ledgerId, int year)
        {
            string query = @"
                WITH ""AccountBalances"" AS (
                    SELECT 
                        a.""Id"" AS AccountId,
                        a.""Title"" AS AccountTitle,
                        at.""Title"" AS AccountTypeName,
                        COALESCE(SUM(
                            CASE 
                                WHEN tt.""Title"" = '@TTTIncome' THEN t.""Amount""
                                WHEN tt.""Title"" = '@TTTExpense' THEN -t.""Amount""
                                WHEN tt.""Title"" = '@TTTTransfer' THEN 0
                                ELSE 0
                            END
                        ), 0) AS RawNetChange,
                        c.""FractionalUnitFactor"" AS FractionalUnitFactor
                    FROM ""Account"" a
                    JOIN ""AccountType"" at ON a.""AccountTypeId"" = at.""Id""
                    JOIN ""UserLedger"" ul ON a.""UserId"" = ul.""UserId""
                    JOIN ""Currency"" c ON a.""CurrencyId"" = c.""Id""
                    LEFT JOIN ""Transaction"" t ON a.""Id"" = t.""AccountId"" 
                        AND t.""IsDeleted"" = false
                        AND EXTRACT(YEAR FROM t.""Date"") = @Year
                    LEFT JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                    WHERE ul.""LedgerId"" = @LedgerId 
                      AND a.""IsDeleted"" = false
                      AND ul.""IsDeleted"" = false
                    GROUP BY a.""Id"", a.""Title"", at.""Title"", c.""FractionalUnitFactor""
                )
                SELECT 
                    AccountId,
                    AccountTitle,
                    AccountTypeName,
                    RawNetChange,
                    FractionalUnitFactor
                FROM ""AccountBalances""
                ORDER BY AccountTypeName, AccountTitle
            ";

            var qparams = new
            {
                LedgerId = ledgerId,
                Year = year,
                TTTIncome = TransactionTypes.Income,
                TTTExpense = TransactionTypes.Expense,
                TTTTransfer = TransactionTypes.Transfer,
            };
            _logger.LogQuery(query, qparams);

            var results = await _unitOfWork.Connection.QueryAsync<HistoricalAccountDataRawDto>(query, qparams, _unitOfWork.Transaction);
            
            // Convert raw amounts to decimal
            return results.Select(r => new HistoricalAccountDataDto
            {
                AccountId = r.AccountId,
                AccountTitle = r.AccountTitle,
                AccountTypeName = r.AccountTypeName,
                NetChange = (decimal)r.RawNetChange / (r.FractionalUnitFactor == 0 ? 1 : r.FractionalUnitFactor)
            });
        }
    }
} 