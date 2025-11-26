using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.UserLedgerRoles;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class UserLedgerRoleQueryService : IUserLedgerRoleQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserLedgerRoleQueryService> _logger;

        public UserLedgerRoleQueryService(IUnitOfWork unitOfWork, ILogger<UserLedgerRoleQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<UserLedgerRoleDto>> GetUserLedgerRolesAsync()
        {
            string query = @"
                SELECT ""Id"", ""Title""
                FROM ""UserLedgerRole""
                ORDER BY ""Title""
                ";

            _logger.LogQuery(query, null);

            return await _unitOfWork.Connection.QueryAsync<UserLedgerRoleDto>(query, null, _unitOfWork.Transaction);
        }

        public async Task<UserLedgerRoleDto?> GetUserLedgerRoleByTitleAsync(string title)
        {
            string query = @"
                SELECT ""Id"", ""Title""
                FROM ""UserLedgerRole""
                WHERE ""Title"" LIKE @Title
                ";

            _logger.LogQuery(query, new { Title = title });

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserLedgerRoleDto>(
                query,
                new { Title = title },
                _unitOfWork.Transaction);
        }
    }
}