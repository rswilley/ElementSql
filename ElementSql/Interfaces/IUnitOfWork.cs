using System.Data;

namespace ElementSql.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        void BeginTransaction();
        void Commit();
        void RollBack();

        IDbTransaction GetTransaction();
        IDbConnection GetConnection();
    }
}
