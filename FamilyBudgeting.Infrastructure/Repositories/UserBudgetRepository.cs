using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.UserBudgets;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class UserBudgetRepository : IUserBudgetRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserBudgetRepository> _logger;

        public UserBudgetRepository(IUnitOfWork unitOfWork, ILogger<UserBudgetRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateUserBudgetAsync(UserBudget userBudget)
        {
            string query = @"
                INSERT INTO ""UserBudget""
                       (""UserId"", ""BudgetId"", ""RoleId"", ""CreatedAt"")
                VALUES
                       (@UserId, @BudgetId, @RoleId, @CreatedAt)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                UserId = userBudget.UserId,
                BudgetId = userBudget.BudgetId,
                RoleId = userBudget.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateUserBudgetAsync(Guid id, UserBudget userBudget)
        {
            string query = @"
                UPDATE ""UserLedger""
                SET 
                    ""UserId"" = @UserId,
                    ""BudgetId"" = @BudgetId,
                    ""RoleId"" = @RoleId,
                    ""UpdatedAt"" = @UpdatedAt
                    ""IsDeleted"" = @IsDeleted;
                WHERE 
                    ""Id"" = @Id;
            ";

            var qparams = new
            {
                Id = id,
                UserId = userBudget.UserId,
                LedgerId = userBudget.BudgetId,
                RoleId = userBudget.RoleId,
                UpdatedAt = userBudget.UpdatedAt,
                IsDeleted = userBudget.IsDeleted
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction) > 0;
        }
    }
} 