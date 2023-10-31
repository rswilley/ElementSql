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

        public async Task<IConnectionContext> StartSession()
        {
            var session = _serviceProvider.GetRequiredService<IConnectionSession>();
            await session.BeginSessionAsync();
            return new SessionContext(session);
        }

        public async Task<IConnectionContext> StartUnitOfWork()
        {
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            await unitOfWork.BeginTransactionAsync();
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
    }
}