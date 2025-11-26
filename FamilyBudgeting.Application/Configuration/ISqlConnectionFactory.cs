using System.Data;

namespace FamilyBudgeting.Domain.Core
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetOpenConnection();
        IDbTransaction BeginTransaction();
        IDbTransaction GetCurrentTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        void Dispose();
    }
}
