using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.Users;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class UserSettingsQueryService : IUserSettingsQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserQueryService> _logger;

        public UserSettingsQueryService(IUnitOfWork unitOfWork, ILogger<UserQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserSettingsDto?> GetUserSettingsAsync(Guid userId)
        {
            string query = @"
                SELECT ""Id"", ""UserId"", ""MainCurrencyId"", ""ShowOnboarding"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted""
                FROM ""UserSettings""
                WHERE ""UserId"" = @UserId AND ""IsDeleted"" = false;
                ";

            var qparams = new { UserId = userId };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<UserSettingsDto>(
                query,
                qparams,
                _unitOfWork.Transaction
            );
        }
    }
}
