using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Constants;
using FamilyBudgeting.Domain.DTOs.Models.Transactions;
using FamilyBudgeting.Domain.DTOs.Requests.Transactions;
using FamilyBudgeting.Domain.DTOs.Responses.Transactions;
using FamilyBudgeting.Domain.Interfaces.Queries;
using Microsoft.Extensions.Logging;
using FamilyBudgeting.Infrastructure.Extensions;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class TransactionQueryService : ITransactionQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionQueryService> _logger;

        public TransactionQueryService(IUnitOfWork unitOfWork, ILogger<TransactionQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsFromLedgerAsync(Guid? ledgerId,
            DateTime startDate, DateTime endDate, Guid? budgetId)
        {
            string query = @"
                SELECT 
                    ""Id"", ""AccountId"", ""LedgerId"", ""TransactionTypeId"", ""CategoryId"",
                    ""CurrencyId"", ""Amount"", ""Date"", ""Note"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""UserId"", ""BudgetCategoryId""
                FROM ""Transaction""
                WHERE ""IsDeleted"" = false
                AND ""Date"" BETWEEN @StartDate AND @EndDate
                {0}  -- LedgerId condition
                {1}  -- BudgetId condition
                ORDER BY ""Date"" DESC
                ";

            string ledgerCondition = ledgerId.HasValue ? @"AND ""LedgerId"" = @LedgerId" : string.Empty;
            string budgetCondition = budgetId.HasValue ? @"AND ""BudgetId"" = @BudgetId" : string.Empty;
            query = string.Format(query, ledgerCondition, budgetCondition);

            var qparams = new
            {
                LedgerId = ledgerId,
                StartDate = startDate,
                EndDate = endDate,
                BudgetId = budgetId
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryAsync<TransactionDto>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<GetTransactionListResponse>> GetTransactionListAsync(
            Guid? ledgerId,
            DateTime startDate,
            DateTime endDate,
            Guid? budgetId,
            Guid? categoryId,
            Guid? budgetCategoryId,
            Guid? userId = null)
        {
            string query = @"
                SELECT
                    t.""Id"",
                    t.""AccountId"",
                    a.""Title"" as AccountTitle,
                    u.""Id"" AS UserId, 
                    u.""FirstName"" || ' ' || u.""LastName"" AS UserFullName,
                    t.""LedgerId"",
                    t.""TransactionTypeId"",
                    tt.""Title"" AS TransactionTypeName,
                    bc.""Id"" AS BudgetCategoryId,
                    catFromBc.""Title"" AS BudgetCategoryName,
                    t.""CategoryId"",
                    cat.""Title"" AS CategoryName,  
                    t.""CurrencyId"",
                    cur.""Name"" AS CurrencyTitle,
                    cur.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor,
                    t.""BudgetId"",
                    t.""Amount"",
                    t.""Date"",
                    t.""Note""
                FROM ""Transaction"" t
                JOIN ""Account"" a ON a.""Id"" = t.""AccountId""
                JOIN ""AspNetUsers"" u ON t.""UserId"" = u.""Id""
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                LEFT JOIN ""Category"" cat ON t.""CategoryId"" = cat.""Id""
                LEFT JOIN ""BudgetCategory"" bc ON t.""BudgetCategoryId"" = bc.""Id""
                LEFT JOIN ""Category"" catFromBc ON bc.""CategoryId"" = catFromBc.""Id""
                JOIN ""Currency"" cur ON t.""CurrencyId"" = cur.""Id""
                {0}  -- UserLedger join for access control
                WHERE t.""IsDeleted"" = false
                AND t.""Date"" BETWEEN @StartDate AND @EndDate
                {1}  -- LedgerId condition
                {2}  -- BudgetId condition
                {3}  -- CategoryId condition
                {4}  -- BudgetCategoryId condition
                ORDER BY t.""Date"" DESC";

            string userLedgerJoin = ledgerId.HasValue ? "" : @"JOIN ""UserLedger"" ul ON t.""LedgerId"" = ul.""LedgerId""";
            string ledgerCondition = ledgerId.HasValue ? @"AND t.""LedgerId"" = @LedgerId" : @"AND ul.""UserId"" = @UserId";
            string budgetCondition = budgetId.HasValue ? @"AND t.""BudgetId"" = @BudgetId" : string.Empty;
            string categoryCondition = categoryId.HasValue ? @"AND t.""CategoryId"" = @CategoryId" : string.Empty;
            string budgetCategoryCondition = budgetCategoryId.HasValue ? @"AND t.""BudgetCategoryId"" = @BudgetCategoryId" : string.Empty;
            query = string.Format(query, userLedgerJoin, ledgerCondition, budgetCondition, categoryCondition, budgetCategoryCondition);

            var qparams = new
            {
                LedgerId = ledgerId,
                StartDate = startDate,
                EndDate = endDate,
                BudgetId = budgetId,
                CategoryId = categoryId,
                BudgetCategoryId = budgetCategoryId,
                UserId = userId
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryAsync<GetTransactionListResponse>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<(IEnumerable<GetTransactionListResponse> Items, int TotalCount)> GetTransactionListPaginatedAsync(
            Guid? ledgerId,
            DateTime startDate,
            DateTime endDate,
            Guid? budgetId,
            Guid? categoryId,
            Guid? budgetCategoryId,
            Guid? userId,
            int page,
            int pageSize)
        {
            // Count query
            string countQuery = @"
                SELECT COUNT(*)
                FROM ""Transaction"" t
                {0}  -- UserLedger join for access control
                WHERE t.""IsDeleted"" = false
                AND t.""Date"" BETWEEN @StartDate AND @EndDate
                {1}  -- LedgerId condition
                {2}  -- BudgetId condition
                {3}  -- CategoryId condition
                {4}  -- BudgetCategoryId condition
            ";

            string userLedgerJoin = ledgerId.HasValue ? "" : @"JOIN ""UserLedger"" ul ON t.""LedgerId"" = ul.""LedgerId""";
            string ledgerCondition = ledgerId.HasValue ? @"AND t.""LedgerId"" = @LedgerId" : @"AND ul.""UserId"" = @UserId";
            string budgetCondition = budgetId.HasValue ? @"AND t.""BudgetId"" = @BudgetId" : string.Empty;
            string categoryCondition = categoryId.HasValue ? @"AND t.""CategoryId"" = @CategoryId" : string.Empty;
            string budgetCategoryCondition = budgetCategoryId.HasValue ? @"AND t.""BudgetCategoryId"" = @BudgetCategoryId" : string.Empty;
            countQuery = string.Format(countQuery, userLedgerJoin, ledgerCondition, budgetCondition, categoryCondition, budgetCategoryCondition);

            var countParams = new
            {
                LedgerId = ledgerId,
                StartDate = startDate,
                EndDate = endDate,
                BudgetId = budgetId,
                CategoryId = categoryId,
                BudgetCategoryId = budgetCategoryId,
                UserId = userId
            };

            _logger.LogQuery(countQuery, countParams);
            int totalCount = await _unitOfWork.Connection.ExecuteScalarAsync<int>(countQuery, countParams, _unitOfWork.Transaction);

            // Data query
            string dataQuery = @"
                SELECT
                    t.""Id"",
                    t.""AccountId"",
                    a.""Title"" as AccountTitle,
                    u.""Id"" AS UserId, 
                    u.""FirstName"" || ' ' || u.""LastName"" AS UserFullName,
                    t.""LedgerId"",
                    t.""TransactionTypeId"",
                    tt.""Title"" AS TransactionTypeName,
                    bc.""Id"" AS BudgetCategoryId,
                    catFromBc.""Title"" AS BudgetCategoryName,
                    t.""CategoryId"",
                    cat.""Title"" AS CategoryName,  
                    t.""CurrencyId"",
                    cur.""Name"" AS CurrencyTitle,
                    cur.""FractionalUnitFactor"" AS CurrencyFractionalUnitFactor,
                    t.""BudgetId"",
                    t.""Amount"",
                    t.""Date"",
                    t.""Note""
                FROM ""Transaction"" t
                JOIN ""Account"" a ON a.""Id"" = t.""AccountId""
                JOIN ""AspNetUsers"" u ON t.""UserId"" = u.""Id""
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                LEFT JOIN ""Category"" cat ON t.""CategoryId"" = cat.""Id""
                LEFT JOIN ""BudgetCategory"" bc ON t.""BudgetCategoryId"" = bc.""Id""
                LEFT JOIN ""Category"" catFromBc ON bc.""CategoryId"" = catFromBc.""Id""
                JOIN ""Currency"" cur ON t.""CurrencyId"" = cur.""Id""
                {0}  -- UserLedger join for access control
                WHERE t.""IsDeleted"" = false
                AND t.""Date"" BETWEEN @StartDate AND @EndDate
                {1}  -- LedgerId condition
                {2}  -- BudgetId condition
                {3}  -- CategoryId condition
                {4}  -- BudgetCategoryId condition
                ORDER BY t.""Date"" DESC
            ";

            if (!(page == 0 && pageSize == 0))
            {
                dataQuery += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY ";
            }

            dataQuery = string.Format(dataQuery, userLedgerJoin, ledgerCondition, budgetCondition, categoryCondition, budgetCategoryCondition);

            var dataParams = new
            {
                LedgerId = ledgerId,
                StartDate = startDate,
                EndDate = endDate,
                BudgetId = budgetId,
                CategoryId = categoryId,
                BudgetCategoryId = budgetCategoryId,
                UserId = userId,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };

            IEnumerable<GetTransactionListResponse> items;
            if (page == 0 && pageSize == 0)
            {
                _logger.LogQuery(dataQuery, dataParams);
                items = await _unitOfWork.Connection.QueryAsync<GetTransactionListResponse>(dataQuery, dataParams, _unitOfWork.Transaction);
            }
            else
            {
                _logger.LogQuery(dataQuery, dataParams);
                items = await _unitOfWork.Connection.QueryAsync<GetTransactionListResponse>(dataQuery, dataParams, _unitOfWork.Transaction);
            }

            return (items, totalCount);
        }

        public async Task<GetTransactionListResponse_Summary> GetTransactionSummaryAsync(
            Guid? ledgerId,
            DateTime startDate,
            DateTime endDate,
            Guid? budgetId,
            Guid? categoryId,
            Guid? budgetCategoryId,
            Guid? userId)
        {
            string summaryQuery = @"
                SELECT 
                    COALESCE(SUM(CASE WHEN tt.""Title"" = @TTTIncome THEN t.""Amount"" ELSE 0 END), 0) AS ""TotalAmountIncome"",
                    COALESCE(SUM(CASE WHEN tt.""Title"" = @TTTExpense THEN t.""Amount"" ELSE 0 END), 0) AS ""TotalAmountExpense"",
                    COALESCE(SUM(CASE WHEN tt.""Title"" = @TTTIncome THEN t.""Amount"" 
                                     WHEN tt.""Title"" = @TTTExpense THEN -t.""Amount""
                                     ELSE 0 END), 0) AS ""TotalAmountNet"",
                    t.""CurrencyId"",
                    cur.""Name"" AS ""CurrencyTitle"",
                    cur.""FractionalUnitFactor"" AS ""CurrencyFractionalUnitFactor""
                FROM ""Transaction"" t
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                JOIN ""Currency"" cur ON t.""CurrencyId"" = cur.""Id""
                {0}  -- UserLedger join for access control
                WHERE t.""IsDeleted"" = false
                AND t.""Date"" BETWEEN @StartDate AND @EndDate
                {1}  -- LedgerId condition
                {2}  -- BudgetId condition
                {3}  -- CategoryId condition
                {4}  -- BudgetCategoryId condition
                GROUP BY t.""CurrencyId"", cur.""Name"", cur.""FractionalUnitFactor""
                LIMIT 1
            ";

            string userLedgerJoin = ledgerId.HasValue ? "" : @"JOIN ""UserLedger"" ul ON t.""LedgerId"" = ul.""LedgerId""";
            string ledgerCondition = ledgerId.HasValue ? @"AND t.""LedgerId"" = @LedgerId" : @"AND ul.""UserId"" = @UserId";
            string budgetCondition = budgetId.HasValue ? @"AND t.""BudgetId"" = @BudgetId" : string.Empty;
            string categoryCondition = categoryId.HasValue ? @"AND t.""CategoryId"" = @CategoryId" : string.Empty;
            string budgetCategoryCondition = budgetCategoryId.HasValue ? @"AND t.""BudgetCategoryId"" = @BudgetCategoryId" : string.Empty;

            summaryQuery = string.Format(summaryQuery, userLedgerJoin, ledgerCondition, budgetCondition, categoryCondition, budgetCategoryCondition);

            var parameters = new
            {
                LedgerId = ledgerId,
                StartDate = startDate,
                EndDate = endDate,
                BudgetId = budgetId,
                CategoryId = categoryId,
                BudgetCategoryId = budgetCategoryId,
                UserId = userId,
                TTTIncome = TransactionTypes.Income,
                TTTExpense = TransactionTypes.Expense,
                TTTTransfer = TransactionTypes.Transfer
            };

            _logger.LogQuery(summaryQuery, parameters);
            var summary = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<GetTransactionListResponse_Summary>(
                summaryQuery,
                parameters,
                _unitOfWork.Transaction);

            // Return default summary if no transactions found
            return summary ?? new GetTransactionListResponse_Summary
            {
                TotalAmountIncome = 0,
                TotalAmountExpense = 0,
                TotalAmountNet = 0,
                CurrencyId = Guid.Empty,
                CurrencyTitle = string.Empty,
                CurrencyFractionalUnitFactor = 100
            };
        }

        public IQueryBuilder<TransactionDto?> GetTransactionById(Guid transactionId)
        {
            string query = @"
                SELECT 
                    ""Id"", ""AccountId"", ""LedgerId"", ""TransactionTypeId"", ""CategoryId"",
                    ""CurrencyId"", ""Amount"", ""Date"", ""Note"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"", ""UserId"", ""BudgetCategoryId""
                FROM ""Transaction""
                WHERE ""Id"" = @TransactionId
                AND ""IsDeleted"" = false
                ";

            var qparams = new
            {
                TransactionId = transactionId,
            };

            _logger.LogQuery(query, qparams);

            return _unitOfWork.CreateQueryBuilder<TransactionDto>(query, qparams, _logger);
        }

        public async Task<LedgerTransactionStatisticsResponse> GetLedgerTransactionStatisticsAsync(GetLedgerTransactionStatisticsRequest request)
        {
            string query = @"
                SELECT 
                    t.""Id"",
                    t.""Amount"", 
                    t.""CurrencyId"",
                    c.""FractionalUnitFactor"",
                    t.""TransactionTypeId"", 
                    tt.""Title"" AS TransactionTypeName, 
                    t.""CategoryId"", 
                    COALESCE(cat.""Title"", 'Uncategorized') AS CategoryName,
                    t.""BudgetId"",
                    t.""BudgetCategoryId""
                FROM ""Transaction"" t
                JOIN ""TransactionType"" tt ON t.""TransactionTypeId"" = tt.""Id""
                JOIN ""Currency"" c ON t.""CurrencyId"" = c.""Id""
                LEFT JOIN ""Category"" cat ON t.""CategoryId"" = cat.""Id""
                WHERE t.""LedgerId"" = @LedgerId
                  AND t.""IsDeleted"" = false
                  AND t.""Date"" >= @StartDate 
                  AND t.""Date"" <= @EndDate
                  {0}  -- Budget condition
            ";

            string budgetCondition = request.BudgetId.HasValue
                    ? @"AND (t.""BudgetId"" = @BudgetId OR t.""BudgetCategoryId"" IN (
                    SELECT ""Id"" FROM ""BudgetCategory"" WHERE ""BudgetId"" = @BudgetId
               ))"
                : string.Empty;

            query = string.Format(query, budgetCondition);

            var qparams = new
            {
                LedgerId = request.LedgerId,
                StartDate = request.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = request.EndDate.ToDateTime(TimeOnly.MaxValue),
                BudgetId = request.BudgetId
            };

            _logger.LogQuery(query, qparams);

            var transactions = await _unitOfWork.Connection.QueryAsync<TransactionRecord>(query, qparams, _unitOfWork.Transaction);

            var response = new LedgerTransactionStatisticsResponse();
            response.TransactionCount = transactions.Count();

            // Convert amounts to decimal using fractional unit factor
            var normalizedTransactions = transactions.Select(t => new
            {
                t.TransactionTypeId,
                t.TransactionTypeName,
                t.CategoryId,
                t.CategoryName,
                NormalizedAmount = (decimal)t.Amount / (t.FractionalUnitFactor == 0 ? 1 : t.FractionalUnitFactor)
            }).ToList();

            // Calculate totals based on transaction type
            var incomeTypes = new[] { TransactionTypes.Income, TransactionTypes.Transfer }; // Adjust based on your transaction types
            response.FormattedTotalIncome = normalizedTransactions
                .Where(t => incomeTypes.Contains(t.TransactionTypeName))
                .Sum(t => t.NormalizedAmount);

            response.FormattedTotalExpenses = normalizedTransactions
                .Where(t => !incomeTypes.Contains(t.TransactionTypeName))
                .Sum(t => t.NormalizedAmount);

            // Breakdown by category
            var categoryGroups = normalizedTransactions
                .GroupBy(t => new { t.CategoryId, t.CategoryName, t.TransactionTypeName })
                .Where(g => g.Key.CategoryId != null); // Exclude null categories if needed

            foreach (var group in categoryGroups)
            {
                response.CategoryBreakdowns.Add(new CategoryBreakdown
                {
                    CategoryId = group.Key.CategoryId ?? Guid.Empty,
                    CategoryName = group.Key.CategoryName,
                    FormattedTotalAmount = group.Sum(t => t.NormalizedAmount),
                    TransactionCount = group.Count(),
                    TransactionTypeName = group.Key.TransactionTypeName,
                });
            }

            // Breakdown by transaction type
            var typeGroups = normalizedTransactions.GroupBy(t => new { t.TransactionTypeId, t.TransactionTypeName });
            foreach (var group in typeGroups)
            {
                response.TransactionTypeBreakdowns.Add(new TransactionTypeBreakdown
                {
                    TransactionTypeId = group.Key.TransactionTypeId,
                    TransactionTypeName = group.Key.TransactionTypeName,
                    FormattedTotalAmount = group.Sum(t => t.NormalizedAmount),
                    TransactionCount = group.Count()
                });
            }

            return response;
        }

        private class TransactionRecord
        {
            public Guid Id { get; set; }
            public int Amount { get; set; }
            public Guid CurrencyId { get; set; }
            public int FractionalUnitFactor { get; set; }
            public Guid TransactionTypeId { get; set; }
            public string TransactionTypeName { get; set; }
            public Guid? CategoryId { get; set; }
            public string CategoryName { get; set; }
            public Guid? BudgetId { get; set; }
            public Guid? BudgetCategoryId { get; set; }
        }
    }
}