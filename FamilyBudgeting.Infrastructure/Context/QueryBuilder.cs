using Dapper;
using FamilyBudgeting.Domain.Core;
using FamilyBudgeting.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;

namespace FamilyBudgeting.Infrastructure.Context
{
    public class QueryBuilder<T> : IQueryBuilder<T> where T : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;
        private readonly string _sql;
        private readonly object _parameters;
        private string _lockClause = string.Empty;

        public QueryBuilder(IUnitOfWork unitOfWork, ILogger logger, string sql, object parameters)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sql = sql ?? throw new ArgumentNullException(nameof(sql));
            _parameters = parameters;
        }

        public IQueryBuilder<T> ForUpdate()
        {
            ValidateTransaction();
            _lockClause = " FOR UPDATE";
            return this;
        }

        public IQueryBuilder<T> ForNoKeyUpdate()
        {
            ValidateTransaction();
            _lockClause = " FOR NO KEY UPDATE";
            return this;
        }

        public IQueryBuilder<T> SkipLocked()
        {
            ValidateTransaction();
            _lockClause = " FOR UPDATE SKIP LOCKED";
            return this;
        }

        public async Task<T?> QueryFirstOrDefaultAsync()
        {
            string finalSql = string.Empty;
            if (_sql.Contains(";"))
            {
                finalSql = _sql.Replace(";", _lockClause);
            }
            else
            {
                finalSql = _sql + _lockClause;
            }
             
            _logger.LogQuery(finalSql, _parameters);
            return await _unitOfWork.Connection.QueryFirstOrDefaultAsync<T>(
                finalSql, _parameters, _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<T>> QueryManyAsync()
        {
            string finalSql = _sql + _lockClause;
            _logger.LogQuery(finalSql, _parameters);
            return await _unitOfWork.Connection.QueryAsync<T>(
                finalSql, _parameters, _unitOfWork.Transaction);
        }

        private void ValidateTransaction()
        {
            if (_unitOfWork.Transaction == null)
            {
                throw new InvalidOperationException("Locking operations require an active transaction.");
            }
        }
    }
}
