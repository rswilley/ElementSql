using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ElementSql.MySqlTests")]

namespace ElementSql.Interfaces
{
    public interface IStorageManager
    {
        Task<IConnectionContext> StartSession(string databaseName = "Default");
        Task<IConnectionContext> StartUnitOfWork(string databaseName = "Default");
        TRepository GetRepository<TRepository>() where TRepository : ISqlRepository;
        TQuery GetQuery<TQuery>() where TQuery : ISqlQuery;
    }
}
