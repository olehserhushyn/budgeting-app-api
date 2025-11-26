using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.Data.Ledgers;
using FamilyBudgeting.Domain.Data.UserLedgers;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Repositories
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LedgerRepository> _logger;

        public LedgerRepository(IUnitOfWork unitOfWork, ILogger<LedgerRepository> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<(Guid LedgerId, Guid UserLedgerId)> CreateLedgerAsync(Ledger ledger, UserLedger uLedger)
        {
            try
            {
                // Create Ledger
                string createLedgerQuery = @"
                    INSERT INTO ""Ledger""
                    (""Title"", ""CreatedAt"", ""UpdatedAt"")
                    VALUES
                    (@Title, @CreatedAt, @UpdatedAt)
                    RETURNING ""Id"";
                ";

                var lparams = new
                {
                    Title = ledger.Title,
                    CreatedAt = ledger.CreatedAt,
                    UpdatedAt = ledger.UpdatedAt
                };

                _logger.LogQuery(createLedgerQuery, lparams);

                Guid ledgerId = await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(
                    createLedgerQuery,
                    lparams,
                    _unitOfWork.Transaction
                );

                // Create UserLedger
                string createUserLedgerQuery = @"
                    INSERT INTO ""UserLedger""
                    (""UserId"", ""LedgerId"", ""RoleId"", ""CreatedAt"")
                    VALUES
                    (@UserId, @LedgerId, @RoleId, @CreatedAt)
                    RETURNING ""Id"";
                ";

                var qparams = new
                {
                    UserId = uLedger.UserId,
                    LedgerId = ledgerId,
                    RoleId = uLedger.RoleId,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogQuery(createUserLedgerQuery, qparams);

                Guid userLedgerId = await _unitOfWork.Connection.ExecuteScalarAsync<Guid>(
                    createUserLedgerQuery,
                    qparams,
                    _unitOfWork.Transaction
                );

                return (ledgerId, userLedgerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Transaction failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateLedgerAsync(Ledger ledger)
        {
            string updateQuery = @"
                UPDATE ""Ledger"" 
                SET ""Title"" = @Title, 
                    ""UpdatedAt"" = @UpdatedAt,
                    ""IsDeleted"" = @IsDeleted
                WHERE ""Id"" = @Id;";

            var parameters = new
            {
                Id = ledger.Id,
                Title = ledger.Title,
                UpdatedAt = ledger.UpdatedAt,
                IsDeleted = ledger.IsDeleted,
            };

            _logger.LogQuery(updateQuery, parameters);
            int rows = await _unitOfWork.Connection.ExecuteAsync(updateQuery, parameters, _unitOfWork.Transaction);
            return rows > 0;
        }
    }
}