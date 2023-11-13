using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ElementSql.MySqlTests")]

namespace ElementSql.Interfaces
{
    public interface IStorageManager
    {
        Task<IConnectionContext> StartSessionAsync(string databaseName = "");
        Task<IConnectionContext> StartUnitOfWorkAsync(string databaseName = "");
        IConnectionContext StartSession(string databaseName = "");
        IConnectionContext StartUnitOfWork(string databaseName = "");
        TRepository GetRepository<TRepository>() where TRepository : IElementSqlRepository;
        TQuery GetQuery<TQuery>() where TQuery : IElementSqlQuery;
    }
}
