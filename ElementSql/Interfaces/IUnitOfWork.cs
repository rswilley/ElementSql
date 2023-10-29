using System.Data;

namespace ElementSql.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        void Commit();
        void RollBack();

        IDbTransaction GetTransaction();
        IDbConnection GetConnection();
    }
}
