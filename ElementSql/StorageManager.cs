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

        public async Task<IConnectionContext> StartSession(string databaseName = "Default")
        {
            var databases = _serviceProvider.GetServices<ISqlDatabase>();
            var selectedDatabase = databases.SingleOrDefault(d => d.Name.Equals(databaseName, StringComparison.InvariantCultureIgnoreCase)) 
                ?? throw new Exception($"Database {databaseName} is not registered.");

            await selectedDatabase.Session.BeginSessionAsync();
            return new SessionContext(selectedDatabase.Session);
        }

        public async Task<IConnectionContext> StartUnitOfWork(string databaseName = "Default")
        {
            var databases = _serviceProvider.GetServices<ISqlDatabase>();
            var selectedDatabase = databases.SingleOrDefault(d => d.Name.Equals(databaseName, StringComparison.InvariantCultureIgnoreCase))
                ?? throw new Exception($"Database {databaseName} is not registered.");

            await selectedDatabase.UnitOfWork.BeginTransactionAsync();
            return new UnitOfWorkContext(selectedDatabase.UnitOfWork);
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
    }
}