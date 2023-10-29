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

        public TRepository GetRepository<TRepository>()
        {
            var repository = _serviceProvider.GetService<TRepository>();
            if (repository == null)
            {
                throw new Exception($"{nameof(TRepository)} is not registered");
            }

            return repository;
        }

        private readonly IServiceProvider _serviceProvider;
    }
}