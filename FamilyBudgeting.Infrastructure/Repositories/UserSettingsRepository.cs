using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.UsersSettings;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserSettingsRepository> _logger;

        public UserSettingsRepository(IUnitOfWork unitOfWork, ILogger<UserSettingsRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateUserSettingsAsync(UserSettings userSettings)
        {
            string query = @"
                INSERT INTO ""UserSettings""
                (""UserId"", ""MainCurrencyId"", ""ShowOnboarding"", ""CreatedAt"", ""UpdatedAt"")
                VALUES
                (@UserId, @MainCurrencyId, @ShowOnboarding, @CreatedAt, @UpdatedAt)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                UserId = userSettings.UserId,
                MainCurrencyId = userSettings.MainCurrencyId,
                ShowOnboarding = userSettings.ShowOnboarding,
                CreatedAt = userSettings.CreatedAt,
                UpdatedAt = userSettings.UpdatedAt
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(
                query,
                qparams,
                _unitOfWork.Transaction
            );
        }

        public async Task<bool> UpdateUserSettingsAsync(Guid settingsId, UserSettings userSettings)
        {
            string query = @"
                UPDATE ""UserSettings""
                SET ""MainCurrencyId"" = @MainCurrencyId,
                    ""ShowOnboarding"" = @ShowOnboarding,
                    ""UpdatedAt"" = @UpdatedAt
                WHERE ""Id"" = @Id;
                ";

            var qparams = new
            {
                Id = settingsId,
                MainCurrencyId = userSettings.MainCurrencyId,
                ShowOnboarding = userSettings.ShowOnboarding,
                UpdatedAt = userSettings.UpdatedAt
            };

            _logger.LogQuery(query, qparams);

            int rows = await _unitOfWork.Connection.ExecuteAsync(query, qparams, _unitOfWork.Transaction);
            return rows > 0;
        }
    }
}
