using FamilyBudgeting.Domain.Core;
using Npgsql;
using System.Data;

namespace FamilyBudgeting.Infrastructure.Context
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly NpgsqlDataSource _dataSource;
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public SqlConnectionFactory(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public IDbConnection GetOpenConnection()
        {
            _connection ??= _dataSource.CreateConnection();
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            return _connection;
        }

        public IDbTransaction BeginTransaction()
        {
            var connection = GetOpenConnection();
            _transaction ??= connection.BeginTransaction();
            return _transaction;
        }

        public IDbTransaction GetCurrentTransaction()
        {
            return _transaction;
        }

        public void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _transaction = null;

            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
            _connection = null;
        }
    }
}
