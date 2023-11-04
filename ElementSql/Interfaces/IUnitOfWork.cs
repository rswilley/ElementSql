using System.Data;

namespace ElementSql.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync(IsolationLevel il = IsolationLevel.Unspecified);
        void BeginTransaction(IsolationLevel il = IsolationLevel.Unspecified);
        void Commit();
        void RollBack();

        IDbTransaction GetTransaction();
        IDbConnection GetConnection();
    }
}
