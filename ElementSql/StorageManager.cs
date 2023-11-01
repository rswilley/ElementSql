using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ElementSql
{
    public class StorageManager : IStorageManager
    {
        public StorageManager(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IConnectionContext> StartSessionAsync(string databaseName = "Default")
        {
            var session = GetConnectionSession(databaseName);
            await session.BeginSessionAsync();
            return new SessionContext(session);
        }

        public async Task<IConnectionContext> StartUnitOfWorkAsync(string databaseName = "Default")
        {
            var unitOfWork = GetUnitOfWork(databaseName);
            await unitOfWork.BeginTransactionAsync();
            return new UnitOfWorkContext(unitOfWork);
        }

        public IConnectionContext StartSession(string databaseName = "Default")
        {
            var session = GetConnectionSession(databaseName);
            session.BeginSession();
            return new SessionContext(session);
        }

        public IConnectionContext StartUnitOfWork(string databaseName = "Default")
        {
            var unitOfWork = GetUnitOfWork(databaseName);
            unitOfWork.BeginTransaction();
            return new UnitOfWorkContext(unitOfWork);
        }

        public TRepository GetRepository<TRepository>() where TRepository : ISqlRepository
        {
            var repository = _serviceProvider.GetService<TRepository>();
            return repository == null 
                ? throw new Exception($"Repository {nameof(TRepository)} is not registered") 
                : repository;
        }

        public TQuery GetQuery<TQuery>() where TQuery : ISqlQuery
        {
            var query = _serviceProvider.GetService<TQuery>();
            return query == null
                ? throw new Exception($"Query {nameof(TQuery)} is not registered")
                : query;
        }

        private readonly IServiceProvider _serviceProvider;

        private ConnectionSession GetConnectionSession(string databaseName)
        {
            var databases = _serviceProvider.GetServices<ISqlDatabase>();
            var selectedDatabase = databases.SingleOrDefault(d => d.Name.Equals(databaseName, StringComparison.InvariantCultureIgnoreCase))
                ?? throw new Exception($"Database {databaseName} is not registered.");

            var dbConnection = selectedDatabase.DbConnection.Invoke();
            var session = new ConnectionSession(dbConnection);
            return session;
        }

        private UnitOfWork GetUnitOfWork(string databaseName)
        {
            var databases = _serviceProvider.GetServices<ISqlDatabase>();
            var selectedDatabase = databases.SingleOrDefault(d => d.Name.Equals(databaseName, StringComparison.InvariantCultureIgnoreCase))
                ?? throw new Exception($"Database {databaseName} is not registered.");

            var dbConnection = selectedDatabase.DbConnection.Invoke();
            return new UnitOfWork(dbConnection);
        }
    }
}