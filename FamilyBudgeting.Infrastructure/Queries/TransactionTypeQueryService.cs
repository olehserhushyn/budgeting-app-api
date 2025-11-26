using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Domain.DTOs.Models.TransactionTypes;
using FamilyBudgeting.Domain.Interfaces.Queries;
using Dapper;
using Microsoft.Extensions.Logging;
using FamilyBudgeting.Infrastructure.Extensions;

namespace FamilyBudgeting.Infrastructure.Queries
{
    public class TransactionTypeQueryService : ITransactionTypeQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionTypeQueryService> _logger;

        public TransactionTypeQueryService(IUnitOfWork unitOfWork, ILogger<TransactionTypeQueryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TransactionTypeDto>> GetTransactionsTypesAsync()
        {
            const string sql = @"
                SELECT tt.""Id"", tt.""Title""
                FROM ""TransactionType"" as tt";

            _logger.LogQuery(sql, null);
            return await _unitOfWork.Connection.QueryAsync<TransactionTypeDto>(sql, null, _unitOfWork.Transaction);
        }

        public async Task<TransactionTypeDto?> GetTransactionTypeAsync(Guid transactionTypeId)
        {
            const string sql = @"
                SELECT tt.""Id"", tt.""Title""
                FROM ""TransactionType"" as tt
                WHERE tt.""Id"" = @TransactionTypeId ";

            var qparams = new { TransactionTypeId = transactionTypeId };
            _logger.LogQuery(sql, qparams);

            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<TransactionTypeDto>(
                sql, qparams, _unitOfWork.Transaction);
        }
    }
}