using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.UserBudgets;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class UserBudgetQueryService : IUserBudgetQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserBudgetQueryService> _logger;

        public UserBudgetQueryService(IUnitOfWork unitOfWork, ILogger<UserBudgetQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> IsUserInLedgerRoleAsync(Guid userId, Guid budgetId, params string[] roleTitles)
        {
            if (roleTitles == null || roleTitles.Length == 0)
            {
                return false;
            }

            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM ""UserBudget"" as ub
                    INNER JOIN ""UserLedgerRole"" as ulr ON ub.""RoleId"" = ulr.""Id""
                    WHERE ub.""UserId"" = @UserId 
                        AND ub.""BudgetId"" = @BudgetId
                        AND ub.""IsDeleted"" = false
                        AND ulr.""Title"" = ANY(@Titles)
                )";

            var parameters = new { UserId = userId, BudgetId = budgetId, Titles = roleTitles };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QuerySingleAsync<bool>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<UserBudgetDto?> GetUserBudgetDtoByIdAsync(Guid budgetId, Guid id)
        {
            const string query = @"
                SELECT ub.""Id"", ub.""UserId"", ub.""BudgetId"", ub.""RoleId"", ulr.""Title"" as RoleTitle
                FROM ""UserBudget"" as ub
                INNER JOIN ""UserLedgerRole"" as ulr ON ub.""RoleId"" = ulr.""Id""
                WHERE ub.""Id"" = @Id 
                    AND ub.""IsDeleted"" = false
                    AND ul.""BudgetId"" = @BudgetId
            ;";

            var parameters = new { Id = id, BudgetId = budgetId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserBudgetDto>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<UserBudgetDto?> GetUserBudgetDtoByUserIdAsync(Guid budgetId, Guid userId)
        {
            const string query = @"
                SELECT ub.""Id"", ub.""UserId"", ub.""BudgetId"", ub.""RoleId"", ulr.""Title"" as RoleTitle
                FROM ""UserBudget"" as ub
                INNER JOIN ""UserLedgerRole"" as ulr ON ub.""RoleId"" = ulr.""Id""
                WHERE ub.""UserId"" = @UserId 
                    AND ub.""IsDeleted"" = false
                    AND ub.""BudgetId"" = @BudgetId
            ;";

            var parameters = new { UserId = userId, BudgetId = budgetId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserBudgetDto>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<UserBudgetDetailsDto>> GetUserBudgetDtoByBudgetIdAsync(Guid budgetId)
        {
            const string query = @"
                SELECT ub.""Id"", ub.""UserId"", ub.""BudgetId"", ub.""RoleId"", ulr.""Title"" as RoleTitle,
                    anu.""FirstName"" as UserFirstName, anu.""LastName"" as UserLastName
                FROM ""UserBudget"" as ub
                INNER JOIN ""UserLedgerRole"" as ulr ON ub.""RoleId"" = ulr.""Id""
                INNER JOIN ""AspNetUsers"" as anu ON ub.""UserId"" = anu.""Id""
                WHERE ub.""IsDeleted"" = false
                    AND ub.""BudgetId"" = @BudgetId
            ;";

            var parameters = new { BudgetId = budgetId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryAsync<UserBudgetDetailsDto>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<UserBudgetDetailsDto>> GetUserLedgerDtoByBudgetIdAsync(Guid budgetId)
        {
            const string query = @"
                SELECT ul.""Id"", ul.""UserId"", b.""Id"" as BudgetId, ul.""RoleId"", ulr.""Title"" as RoleTitle,
                    anu.""FirstName"" as UserFirstName, anu.""LastName"" as UserLastName
                FROM ""Budget"" as b
                INNER JOIN ""UserLedger"" as ul ON ul.""LedgerId"" = b.""LedgerId""
                INNER JOIN ""UserLedgerRole"" as ulr ON ul.""RoleId"" = ulr.""Id""
                INNER JOIN ""AspNetUsers"" as anu ON ul.""UserId"" = anu.""Id""
                WHERE ul.""IsDeleted"" = false
                    AND b.""Id"" = @BudgetId
            ;";

            var parameters = new { BudgetId = budgetId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryAsync<UserBudgetDetailsDto>(query, parameters, _unitOfWork.Transaction);
        }
    }
}
