using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Transactions;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionRepository> _logger;

        public TransactionRepository(IUnitOfWork unitOfWork, ILogger<TransactionRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateTransactionAsync(Transaction transaction)
        {
            string query = @"
                INSERT INTO ""Transaction""
                       (""AccountId"",
                       ""LedgerId"",
                       ""TransactionTypeId"",
                       ""CategoryId"",
                       ""CurrencyId"",
                       ""Amount"",
                       ""Date"",
                       ""Note"",
                       ""UserId"",
                       ""BudgetId"",
                       ""BudgetCategoryId"",
                       ""CreatedAt"",
                       ""UpdatedAt"")
                VALUES
                       (@AccountId,
                       @LedgerId,
                       @TransactionTypeId,
                       @CategoryId,
                       @CurrencyId,
                       @Amount,
                       @Date,
                       @Note,
                       @UserId,
                       @BudgetId,
                       @BudgetCategoryId,
                       @CreatedAt,
                       @UpdatedAt)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                AccountId = transaction.AccountId,
                LedgerId = transaction.LedgerId,
                TransactionTypeId = transaction.TransactionTypeId,
                CategoryId = transaction.CategoryId,
                CurrencyId = transaction.CurrencyId,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Note = transaction.Note,
                UserId = transaction.UserId,
                BudgetId = transaction.BudgetId,
                BudgetCategoryId = transaction.BudgetCategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<int> CreateTransactionsAsync(IEnumerable<Transaction> transactions)
        {
            string query = @"
                INSERT INTO ""Transaction"" (
                    ""AccountId"", ""LedgerId"", ""TransactionTypeId"", ""CategoryId"", ""CurrencyId"", ""Amount"", ""Date"", ""Note"", ""UserId"", ""BudgetId"", ""BudgetCategoryId"", ""CreatedAt"", ""UpdatedAt""
                ) VALUES (
                    @AccountId, @LedgerId, @TransactionTypeId, @CategoryId, @CurrencyId, @Amount, @Date, @Note, @UserId, @BudgetId, @BudgetCategoryId, @CreatedAt, @UpdatedAt
                );";
            var now = DateTime.UtcNow;
            var paramList = transactions.Select(t => new {
                t.AccountId,
                t.LedgerId,
                t.TransactionTypeId,
                t.CategoryId,
                t.CurrencyId,
                t.Amount,
                t.Date,
                t.Note,
                t.UserId,
                t.BudgetId,
                t.BudgetCategoryId,
                CreatedAt = now,
                UpdatedAt = now
            });
            _logger.LogQuery(query, paramList);
            return await _unitOfWork.Connection.ExecuteAsync(query, paramList, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateTransactionAsync(Guid id, Transaction transaction)
        {
            string query = @"
                UPDATE ""Transaction""
                   SET ""AccountId"" = @AccountId,
                       ""LedgerId"" = @LedgerId,
                       ""TransactionTypeId"" = @TransactionTypeId,
                       ""CategoryId"" = @CategoryId,
                       ""CurrencyId"" = @CurrencyId,
                       ""Amount"" = @Amount,
                       ""Date"" = @Date,
                       ""Note"" = @Note,
                       ""UserId"" = @UserId,
                       ""BudgetCategoryId"" = @BudgetCategoryId,
                       ""UpdatedAt"" = @UpdatedAt,
                       ""IsDeleted"" = @IsDeleted
                 WHERE ""Id"" = @Id;
                ";

            var qparams = new
            {
                Id = id,
                AccountId = transaction.AccountId,
                LedgerId = transaction.LedgerId,
                TransactionTypeId = transaction.TransactionTypeId,
                CategoryId = transaction.CategoryId,
                CurrencyId = transaction.CurrencyId,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Note = transaction.Note,
                UserId = transaction.UserId,
                BudgetCategoryId = transaction.BudgetCategoryId,
                UpdatedAt = transaction.UpdatedAt,
                IsDeleted = transaction.IsDeleted,
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction) > 0;
        }
    }
}