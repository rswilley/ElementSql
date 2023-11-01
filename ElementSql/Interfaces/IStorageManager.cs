using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ElementSql.MySqlTests")]

namespace ElementSql.Interfaces
{
    public interface IStorageManager
    {
        Task<IConnectionContext> StartSessionAsync(string databaseName = "Default");
        Task<IConnectionContext> StartUnitOfWorkAsync(string databaseName = "Default");
        IConnectionContext StartSession(string databaseName = "Default");
        IConnectionContext StartUnitOfWork(string databaseName = "Default");
        TRepository GetRepository<TRepository>() where TRepository : ISqlRepository;
        TQuery GetQuery<TQuery>() where TQuery : ISqlQuery;
    }
}
