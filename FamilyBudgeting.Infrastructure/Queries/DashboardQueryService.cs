using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Queries.DashboardDtos;
using FamilyBudgeting.Domain.DTOs.Responses.Dashboard;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class DashboardQueryService : IDashboardQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardQueryService> _logger;

        public DashboardQueryService(IUnitOfWork unitOfWork, ILogger<DashboardQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DashBoardSummaryResponse_TotalBalance> GetTotalBalanceAsync(Guid ledgerId)
        {
            string currentBalanceQuery = @"
                SELECT SUM(a.""Balance"") AS TotalAmount, 
                       c.""Id"" AS CurrencyId, 
                       c.""Name"" AS CurrencyTitle, 
                       c.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor
                FROM ""Account"" a
                JOIN ""Currency"" c ON a.""CurrencyId"" = c.""Id""
                JOIN ""UserLedger"" ul ON a.""UserId"" = ul.""UserId""
                WHERE ul.""LedgerId"" = @LedgerId 
                  AND a.""IsDeleted"" = false
                  AND ul.""IsDeleted"" = false
                GROUP BY c.""Id"", c.""Name"", c.""FractionalUnitFactor""
                ORDER BY SUM(a.""Balance"") DESC
            ";

            var qparams = new { LedgerId = ledgerId };

            _logger.LogQuery(currentBalanceQuery, qparams);

            var currentResult = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<CurrencyBalanceResult>(
                currentBalanceQuery, qparams, _unitOfWork.Transaction);

            if (currentResult == null || currentResult.TotalAmount == null)
            {
                return new DashBoardSummaryResponse_TotalBalance
                {
                    FormattedTotalAmount = 0,
                    CurrencyId = Guid.Empty,
                    CurrencyTitle = string.Empty,
                    CurrencyFractionalUnitFactor = 1,
                    LastMonthPercent = 0
                };
            }

            int currentAmountInt = currentResult.TotalAmount.Value;
            int fractionalFactor = currentResult.CurrencyFractionalUnitFactor == 0 ? 1 : currentResult.CurrencyFractionalUnitFactor;
            decimal currentAmount = (decimal)currentAmountInt / fractionalFactor;

            decimal lastMonthAmount = 0;
            decimal percentChange = 0;

            /*
             WITH ""OpeningBalance"" AS (
                    SELECT COALESCE(SUM(a.""Balance""), 0) AS ""Balance""
                    FROM ""Account"" a
                    JOIN ""UserLedger"" ul ON a.""UserId"" = ul.""UserId""
                    WHERE ul.""LedgerId"" = @LedgerId 
                      AND a.""IsDeleted"" = false
                      AND ul.""IsDeleted"" = false
                      AND a.""CurrencyId"" = @CurrencyId
                ),
                ""NetChange"" AS (
                    SELECT COALESCE(SUM(
                        CASE 
                            WHEN tt.""Title"" = 'Income' THEN t.""Amount""
                            WHEN tt.""Title"" = 'Expense' THEN -t.""Amount""
                            WHEN tt.""Title"" = 'Transfer' THEN 0
                            ELSE 0
                        END), 0) AS ""Change""
                    FROM ""Transaction"" t
                    JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                    JOIN ""UserLedger"" ul ON t.""UserId"" = ul.""UserId""
                    WHERE ul.""LedgerId"" = @LedgerId 
                      AND t.""IsDeleted"" = false
                      AND ul.""IsDeleted"" = false
                      AND t.""CurrencyId"" = @CurrencyId
                      AND t.""Date"" <= (DATE_TRUNC('month', CURRENT_DATE) - INTERVAL '1 day')::date
                )
                SELECT (ob.""Balance"" + COALESCE(nc.""Change"", 0)) AS LastMonthBalance
                FROM ""OpeningBalance"" ob
                LEFT JOIN ""NetChange"" nc ON true;
             */

            string lastMonthBalanceQuery = @"
                WITH ""OpeningBalance"" AS (
                    SELECT COALESCE(SUM(a.""Balance""), 0) AS ""Balance""
                    FROM ""Account"" a
                    WHERE a.""IsDeleted"" = false
                      AND a.""CurrencyId"" = @CurrencyId
                      AND EXISTS (
                          SELECT 1 FROM ""UserLedger"" ul
                          WHERE ul.""UserId"" = a.""UserId""
                            AND ul.""LedgerId"" = @LedgerId
                            AND ul.""IsDeleted"" = false
                      )
                ),
                ""NetChange"" AS (
                    SELECT COALESCE(SUM(
                        CASE 
                            WHEN tt.""Title"" = 'Income' THEN t.""Amount""
                            WHEN tt.""Title"" = 'Expense' THEN -t.""Amount""
                            ELSE 0
                        END), 0) AS ""Change""
                    FROM ""Transaction"" t
                    JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                    WHERE t.""IsDeleted"" = false
                      AND t.""CurrencyId"" = @CurrencyId
                      AND t.""Date"" <= (DATE_TRUNC('month', CURRENT_DATE) - INTERVAL '1 day')::date
                      AND EXISTS (
                          SELECT 1 FROM ""UserLedger"" ul
                          WHERE ul.""UserId"" = t.""UserId""
                            AND ul.""LedgerId"" = @LedgerId
                            AND ul.""IsDeleted"" = false
                      )
                )
                SELECT (ob.""Balance"" + nc.""Change"") AS ""LastMonthBalance""
                FROM ""OpeningBalance"" ob
                CROSS JOIN ""NetChange"" nc;
            ";

            var lastMonthBalanceQueryParams = new { LedgerId = ledgerId, CurrencyId = currentResult.CurrencyId };

            _logger.LogQuery(lastMonthBalanceQuery, lastMonthBalanceQueryParams);

            var lastMonthResult = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<LastMonthBalanceResult>(
                lastMonthBalanceQuery, lastMonthBalanceQueryParams, _unitOfWork.Transaction);

            if (lastMonthResult?.LastMonthBalance != null)
            {
                lastMonthAmount = (decimal)lastMonthResult.LastMonthBalance.Value / fractionalFactor;
                if (lastMonthAmount != 0)
                {
                    percentChange = ((currentAmount - lastMonthAmount) / Math.Abs(lastMonthAmount)) * 100;
                }
                else if (currentAmount != 0)
                {
                    percentChange = 100;
                }
            }

            return new DashBoardSummaryResponse_TotalBalance
            {
                FormattedTotalAmount = Math.Round(currentAmount, 2),
                CurrencyId = currentResult.CurrencyId,
                CurrencyTitle = currentResult.CurrencyTitle,
                CurrencyFractionalUnitFactor = fractionalFactor,
                LastMonthPercent = Math.Round(percentChange, 2)
            };
        }

        public async Task<DashBoardSummaryResponse_MonthlyFlow> GetMonthlyIncomeAsync(Guid ledgerId)
        {
            string currentMonthQuery = @"
                SELECT SUM(t.""Amount"") AS Amount,
                       c.""Id"" AS CurrencyId,
                       c.""Name"" AS CurrencyTitle,
                       c.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor
                FROM ""Transaction"" t
                JOIN ""Currency"" c ON t.""CurrencyId"" = c.""Id""
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                JOIN ""UserLedger"" ul ON t.""UserId"" = ul.""UserId"" AND ul.""LedgerId"" = @LedgerId AND ul.""IsDeleted"" = false
                WHERE t.""IsDeleted"" = false
                  AND tt.""Title"" = 'Income'
                  AND t.""Date"" >= date_trunc('month', CURRENT_DATE)
                  AND t.""Date"" < (date_trunc('month', CURRENT_DATE) + INTERVAL '1 month')
                GROUP BY c.""Id"", c.""Name"", c.""FractionalUnitFactor""
                ORDER BY Amount DESC
            ";

            var currentMonthQueryParams = new { LedgerId = ledgerId };

            _logger.LogQuery(currentMonthQuery, currentMonthQueryParams);

            var currentResult = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<MonthlyFlowResult>(
                currentMonthQuery, currentMonthQueryParams, _unitOfWork.Transaction);

            if (currentResult == null)
            {
                return new DashBoardSummaryResponse_MonthlyFlow();
            }

            int currentAmountInt = currentResult.Amount ?? 0;
            int fractionalFactor = currentResult.CurrencyFractionalUnitFactor == 0 ? 1 : currentResult.CurrencyFractionalUnitFactor;
            decimal currentAmount = (decimal)currentAmountInt / fractionalFactor;

            decimal lastMonthAmount = 0;
            decimal percentChange = 0;

            string lastMonthQuery = @"
                SELECT SUM(t.""Amount"") AS Amount
                FROM ""Transaction"" t
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                JOIN ""UserLedger"" ul ON t.""UserId"" = ul.""UserId""
                WHERE ul.""LedgerId"" = @LedgerId 
                  AND t.""IsDeleted"" = false
                  AND ul.""IsDeleted"" = false
                  AND tt.""Title"" = 'Income'
                  AND t.""CurrencyId"" = @CurrencyId
                  AND DATE_TRUNC('month', t.""Date"") = DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
            ";

            var lastMonthQueryParams = new { LedgerId = ledgerId, CurrencyId = currentResult.CurrencyId };

            _logger.LogQuery(lastMonthQuery, lastMonthQueryParams);

            var lastMonthResult = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<LastMonthBalanceResult>(
                lastMonthQuery, lastMonthQueryParams, _unitOfWork.Transaction);

            if (lastMonthResult?.LastMonthBalance != null)
            {
                lastMonthAmount = (decimal)lastMonthResult.LastMonthBalance.Value / fractionalFactor;
                if (lastMonthAmount != 0)
                {
                    percentChange = ((currentAmount - lastMonthAmount) / Math.Abs(lastMonthAmount)) * 100;
                }
                else if (currentAmount != 0)
                {
                    percentChange = 100;
                }
            }

            return new DashBoardSummaryResponse_MonthlyFlow
            {
                FormattedAmount = Math.Round(currentAmount, 2),
                CurrencyId = currentResult.CurrencyId,
                CurrencyTitle = currentResult.CurrencyTitle,
                CurrencyFractionalUnitFactor = fractionalFactor,
                LastMonthPercent = Math.Round(percentChange, 2)
            };
        }

        public async Task<DashBoardSummaryResponse_MonthlyFlow> GetMonthlyExpenseAsync(Guid ledgerId)
        {
            string currentMonthQuery = @"
                SELECT SUM(ABS(t.""Amount"")) AS Amount, 
                       c.""Id"" AS CurrencyId, 
                       c.""Name"" AS CurrencyTitle, 
                       c.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor
                FROM ""Transaction"" t
                JOIN ""Currency"" c ON t.""CurrencyId"" = c.""Id""
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                JOIN ""UserLedger"" ul ON t.""UserId"" = ul.""UserId""
                WHERE ul.""LedgerId"" = @LedgerId 
                  AND t.""IsDeleted"" = false
                  AND ul.""IsDeleted"" = false
                  AND tt.""Title"" = 'Expense'
                  AND DATE_TRUNC('month', t.""Date"") = DATE_TRUNC('month', CURRENT_DATE)
                GROUP BY c.""Id"", c.""Name"", c.""FractionalUnitFactor""
                ORDER BY SUM(ABS(t.""Amount"")) DESC
            ";

            var currentMonthQueryParams = new { LedgerId = ledgerId };

            _logger.LogQuery(currentMonthQuery, currentMonthQueryParams);

            var currentResult = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<MonthlyFlowResult>(
                currentMonthQuery, currentMonthQueryParams, _unitOfWork.Transaction);

            if (currentResult == null)
            {
                return new DashBoardSummaryResponse_MonthlyFlow();
            }

            int currentAmountInt = currentResult.Amount ?? 0;
            int fractionalFactor = currentResult.CurrencyFractionalUnitFactor == 0 ? 1 : currentResult.CurrencyFractionalUnitFactor;
            decimal currentAmount = (decimal)currentAmountInt / fractionalFactor;

            decimal lastMonthAmount = 0;
            decimal percentChange = 0;

            string lastMonthQuery = @"
                SELECT SUM(ABS(t.""Amount"")) AS Amount
                FROM ""Transaction"" t
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                JOIN ""UserLedger"" ul ON t.""UserId"" = ul.""UserId""
                WHERE ul.""LedgerId"" = @LedgerId 
                  AND t.""IsDeleted"" = false
                  AND ul.""IsDeleted"" = false
                  AND tt.""Title"" = 'Expense'
                  AND t.""CurrencyId"" = @CurrencyId
                  AND DATE_TRUNC('month', t.""Date"") = DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
            ";

            var lastMonthQueryParams = new { LedgerId = ledgerId, CurrencyId = currentResult.CurrencyId };

            _logger.LogQuery(lastMonthQuery, lastMonthQueryParams);

            var lastMonthResult = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<LastMonthBalanceResult>(
                lastMonthQuery, lastMonthQueryParams, _unitOfWork.Transaction);

            if (lastMonthResult?.LastMonthBalance != null)
            {
                lastMonthAmount = (decimal)lastMonthResult.LastMonthBalance.Value / fractionalFactor;
                if (lastMonthAmount != 0)
                {
                    percentChange = ((currentAmount - lastMonthAmount) / Math.Abs(lastMonthAmount)) * 100;
                }
                else if (currentAmount != 0)
                {
                    percentChange = 100;
                }
            }

            return new DashBoardSummaryResponse_MonthlyFlow
            {
                FormattedAmount = Math.Round(currentAmount, 2),
                CurrencyId = currentResult.CurrencyId,
                CurrencyTitle = currentResult.CurrencyTitle,
                CurrencyFractionalUnitFactor = fractionalFactor,
                LastMonthPercent = Math.Round(percentChange, 2)
            };
        }

        public async Task<DashBoardSummaryResponse_Goal> GetGoalsAsync(Guid ledgerId)
        {
            string query = @"
                SELECT COUNT(*) AS Total,
                       SUM(CASE WHEN g.""CurrentAmount"" >= g.""TargetAmount"" THEN 1 ELSE 0 END) AS Done,
                       SUM(CASE WHEN g.""CurrentAmount"" >= g.""TargetAmount"" 
                                AND DATE_TRUNC('month', g.""Deadline"") = DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
                           THEN 1 ELSE 0 END) AS LastMonthDone
                FROM ""Goal"" g
                WHERE g.""LedgerId"" = @LedgerId AND g.""IsDeleted"" = false;
            ";

            var queryParams = new { LedgerId = ledgerId };

            _logger.LogQuery(query, queryParams);

            var result = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<GoalsResult>(
                query, queryParams, _unitOfWork.Transaction);

            if (result == null)
            { 
                return new DashBoardSummaryResponse_Goal();
            }

            return new DashBoardSummaryResponse_Goal
            {
                Total = result.Total,
                Done = result.Done,
                LastMonthDone = result.LastMonthDone,
            };
        }
    }
}