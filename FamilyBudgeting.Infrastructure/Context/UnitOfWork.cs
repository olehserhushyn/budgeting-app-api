using FamilyBudgeting.Domain.Core;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FamilyBudgeting.Infrastructure.Context
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private bool _transactionStarted;

        public IDbConnection Connection => _connectionFactory.GetOpenConnection();
        public IDbTransaction Transaction => _connectionFactory.GetCurrentTransaction();

        public UnitOfWork(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task BeginTransactionAsync()
        {
            if (!_transactionStarted)
            {
                _connectionFactory.BeginTransaction();
                _transactionStarted = true;
            }
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_transactionStarted)
                {
                    _connectionFactory.CommitTransaction();
                    _transactionStarted = false;
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            await Task.CompletedTask;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transactionStarted)
            {
                _connectionFactory.RollbackTransaction();
                _transactionStarted = false;
            }
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _connectionFactory.Dispose();
        }

        public IQueryBuilder<T> CreateQueryBuilder<T>(string sql, object parameters, ILogger logger) where T : class
        {
            return new QueryBuilder<T>(this, logger, sql, parameters);
        }
    }
}
