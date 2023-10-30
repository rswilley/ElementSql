using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ElementSql.MySqlTests")]

namespace ElementSql.Interfaces
{
    public interface IStorageManager
    {
        Task<IConnectionContext> StartSession();
        Task<IConnectionContext> StartUnitOfWork();
        TRepository GetRepository<TRepository>() where TRepository : IRepo;
        TQuery GetQuery<TQuery>() where TQuery : IQuery;
    }
}
