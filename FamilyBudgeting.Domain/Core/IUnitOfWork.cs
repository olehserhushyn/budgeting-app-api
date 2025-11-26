using Microsoft.Extensions.Logging;
using System.Data;

namespace FamilyBudgeting.Domain.Core
{
    public interface IUnitOfWork : IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        IDbConnection Connection { get; } // Expose connection for repositories
        IDbTransaction Transaction { get; } // Expose transaction for repositories
        IQueryBuilder<T> CreateQueryBuilder<T>(string sql, object parameters, ILogger logger) where T : class;
    }
}
