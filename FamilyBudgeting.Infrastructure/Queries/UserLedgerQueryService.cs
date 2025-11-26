using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.UserLedgers;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class UserLedgerQueryService : IUserLedgerQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserLedgerQueryService> _logger;

        public UserLedgerQueryService(IUnitOfWork unitOfWork, ILogger<UserLedgerQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> CheckUserLedgerAccessAsync(Guid userId, Guid ledgerId)
        {
            const string query = @"
                SELECT 1
                FROM ""UserLedger""
                WHERE ""UserId"" = @UserId 
                AND ""LedgerId"" = @LedgerId
                AND ""IsDeleted"" = false
                LIMIT 1";

            var parameters = new { UserId = userId, LedgerId = ledgerId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.ExecuteScalarAsync<bool>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<bool> IsUserInLedgerRoleAsync(Guid userId, Guid ledgerId, params string[] roleTitles)
        {
            if (roleTitles == null || roleTitles.Length == 0)
            {
                return false;
            }

            const string query = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM ""UserLedger"" as ul
                    INNER JOIN ""UserLedgerRole"" as ulr ON ul.""RoleId"" = ulr.""Id""
                    WHERE ul.""UserId"" = @UserId 
                        AND ul.""LedgerId"" = @LedgerId
                        AND ul.""IsDeleted"" = false
                        AND ulr.""Title"" = ANY(@Titles)
                )";

            var parameters = new { UserId = userId, LedgerId = ledgerId, Titles = roleTitles };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QuerySingleAsync<bool>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<UserLedgerDto?> GetUserLedgerDtoByIdAsync(Guid ledgerId, Guid id)
        {
            const string query = @"
                SELECT ul.""Id"", ul.""UserId"", ul.""LedgerId"", ul.""RoleId"", ulr.""Title"" as RoleTitle
                FROM ""UserLedger"" as ul
                INNER JOIN ""UserLedgerRole"" as ulr ON ul.""RoleId"" = ulr.""Id""
                WHERE ul.""Id"" = @Id 
                    AND ul.""IsDeleted"" = false
                    AND ul.""LedgerId"" = @LedgerId
            ;";

            var parameters = new { Id = id, LedgerId = ledgerId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserLedgerDto>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<UserLedgerDto?> GetUserLedgerDtoByUserIdAsync(Guid ledgerId, Guid userId)
        {
            const string query = @"
                SELECT ul.""Id"", ul.""UserId"", ul.""LedgerId"", ul.""RoleId"", ulr.""Title"" as RoleTitle
                FROM ""UserLedger"" as ul
                INNER JOIN ""UserLedgerRole"" as ulr ON ul.""RoleId"" = ulr.""Id""
                WHERE ul.""UserId"" = @UserId 
                    AND ul.""IsDeleted"" = false
                    AND ul.""LedgerId"" = @LedgerId
            ;";

            var parameters = new { UserId = userId, LedgerId = ledgerId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserLedgerDto>(query, parameters, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<UserLedgerDetailsDto>> GetUserLedgerDtoByLedgerIdAsync(Guid ledgerId)
        {
            const string query = @"
                SELECT ul.""Id"", ul.""UserId"", ul.""LedgerId"", ul.""RoleId"", ulr.""Title"" as RoleTitle,
                    anu.""FirstName"" as UserFirstName, anu.""LastName"" as UserLastName
                FROM ""UserLedger"" as ul
                INNER JOIN ""UserLedgerRole"" as ulr ON ul.""RoleId"" = ulr.""Id""
                INNER JOIN ""AspNetUsers"" as anu ON ul.""UserId"" = anu.""Id""
                WHERE ul.""IsDeleted"" = false
                    AND ul.""LedgerId"" = @LedgerId
            ;";

            var parameters = new { LedgerId = ledgerId };

            _logger.LogQuery(query, parameters);

            return await _unitOfWork.Connection.QueryAsync<UserLedgerDetailsDto>(query, parameters, _unitOfWork.Transaction);
        }
    }
}