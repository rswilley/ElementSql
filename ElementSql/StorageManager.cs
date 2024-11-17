using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ElementSql.Tests")]

namespace ElementSql
{
    public class StorageManager : IStorageManager
    {
        public StorageManager(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IConnectionContext> StartSessionAsync(string databaseName = "")
        {
            var session = GetConnectionSession(databaseName);
            await session.BeginSessionAsync();
            return new SessionContext(session);
        }

        public async Task<IConnectionContext> StartUnitOfWorkAsync(string databaseName = "")
        {
            var unitOfWork = GetUnitOfWork(databaseName);
            await unitOfWork.BeginTransactionAsync();
            return new UnitOfWorkContext(unitOfWork);
        }

        public IConnectionContext StartSession(string databaseName = "")
        {
            var session = GetConnectionSession(databaseName);
            session.BeginSession();
            return new SessionContext(session);
        }

        public IConnectionContext StartUnitOfWork(string databaseName = "")
        {
            var unitOfWork = GetUnitOfWork(databaseName);
            unitOfWork.BeginTransaction();
            return new UnitOfWorkContext(unitOfWork);
        }

        public TRepository GetRepository<TRepository>() where TRepository : IElementSqlRepository
        {
            var repository = _serviceProvider.GetService<TRepository>();
            return repository == null 
                ? throw new Exception($"Repository {typeof(TRepository).Name} is not registered") 
                : repository;
        }

        public TQuery GetQuery<TQuery>() where TQuery : IElementSqlQuery
        {
            var query = _serviceProvider.GetService<TQuery>();
            return query == null
                ? throw new Exception($"Query {typeof(TQuery).Name} is not registered")
                : query;
        }

        private readonly IServiceProvider _serviceProvider;

        private ConnectionSession GetConnectionSession(string databaseName)
        {
            var database = GetDatabase(databaseName);
            var dbConnection = database.DbConnection.Invoke();
            var session = new ConnectionSession(dbConnection);
            return session;
        }

        private UnitOfWork GetUnitOfWork(string databaseName)
        {
            var database = GetDatabase(databaseName);
            var dbConnection = database.DbConnection.Invoke();
            return new UnitOfWork(dbConnection);
        }

        private ISqlDatabase GetDatabase(string databaseName)
        {
            var databases = _serviceProvider.GetServices<ISqlDatabase>();

            if (string.IsNullOrEmpty(databaseName))
            {
                return databases.First();
            }

            var selectedDatabase = databases.SingleOrDefault(d => d.Name.Equals(databaseName, StringComparison.InvariantCultureIgnoreCase))
                ?? throw new Exception($"Database {databaseName} is not registered.");

            return selectedDatabase;
        }
    }
}