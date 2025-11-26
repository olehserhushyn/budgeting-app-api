using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.UserLedgers;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class UserLedgerRepository : IUserLedgerRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserLedgerRepository> _logger;

        public UserLedgerRepository(IUnitOfWork unitOfWork, ILogger<UserLedgerRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateUserLedgerAsync(UserLedger uLedger)
        {
            string query = @"
                INSERT INTO ""UserLedger""
                       (""UserId"", ""LedgerId"", ""RoleId"", ""CreatedAt"")
                VALUES
                       (@UserId, @LedgerId, @RoleId, @CreatedAt)
                RETURNING ""Id"";
                ";

            var qparams = new
            {
                UserId = uLedger.UserId,
                LedgerId = uLedger.LedgerId,
                RoleId = uLedger.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogQuery(query, qparams);

            return await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(query, qparams, _unitOfWork.Transaction);
        }

        public async Task<bool> UpdateUserLedgerAsync(Guid id, UserLedger uLedger)
        {
            string query = @"
                UPDATE ""UserLedger""
                SET 
                    ""UserId"" = @UserId,
                    ""LedgerId"" = @LedgerId,
                    ""RoleId"" = @RoleId,
                    ""UpdatedAt"" = @UpdatedAt
                    ""IsDeleted"" = @IsDeleted;
                WHERE 
                    ""Id"" = @Id;
            ";

            var qparams = new
            {
                Id = id,
                UserId = uLedger.UserId,
                LedgerId = uLedger.LedgerId,
                RoleId = uLedger.RoleId,
                UpdatedAt = uLedger.UpdatedAt,
                IsDeleted = uLedger.IsDeleted
            };

            _logger.LogQuery(query, qparams);

            int rowsAffected = await _unitOfWork.Connection.ExecuteAsync(
                query,
                qparams,
                _unitOfWork.Transaction);

            return rowsAffected > 0;
        }
    }
}