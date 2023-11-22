using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;

namespace ElementSql.MySqlTests
{
    public abstract class AutomatedTestBase
    {
        protected AutomatedTestBase()
        {
            Bootstrap();
            SeedTestDatabase();
        }

        protected IServiceProvider ServiceProvider { get; private set; } = null!;
        protected IStorageManager StorageManager { get; private set; } = null!;
        protected IElementRepository ElementRepository { get; private set; } = null!;

        protected async Task<Element> ShouldCreateRecord(Element toInsert, IConnectionContext context)
        {
            var record = await ElementRepository.InsertAsync(toInsert, context);
            return record;
        }

        protected async Task<Element> ShouldReadRecord(int recordId, IConnectionContext context)
        {
            var record = await ElementRepository.GetByIdAsync(recordId, context);
            return record;
        }

        protected async Task<Element> ShouldUpdateRecord(Element recordToUpdate, IConnectionContext context)
        {
            await ElementRepository.UpdateAsync(recordToUpdate, context);
            var updatedRecord = await ElementRepository.GetByIdAsync(recordToUpdate.Id, context);
            return updatedRecord;
        }

        protected async Task<IEnumerable<Element>> ShouldGetAllRecords(IConnectionContext context)
        {
            var allRecords = await ElementRepository.GetAllAsync(context);
            return allRecords;
        }

        protected async Task<Element> ShouldDeleteRecord(Element recordToDelete, IConnectionContext context)
        {
            await ElementRepository.DeleteAsync(recordToDelete, context);
            var deleted = await ElementRepository.GetByIdAsync(recordToDelete.Id, context);
            return deleted;
        }

        private void Bootstrap()
        {
            var serviceCollection = new ServiceCollection();

            var connectionString = Environment.GetEnvironmentVariable("MySqlConnectionString")!;
            serviceCollection.AddElementSql(config =>
            {
                config.Databases.Add("Default", () => new MySqlConnection(connectionString));
                config.Registration = new ElementSqlRegistration
                {
                    Autoregister = true,
                    AssemblyLocation = typeof(AutomatedTestBase),
                    ServiceLifetime = ServiceLifetime.Singleton
                };
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void SeedTestDatabase()
        {
            // Populate test data
            StorageManager = ServiceProvider.GetRequiredService<IStorageManager>();
            ElementRepository = StorageManager.GetRepository<IElementRepository>();

            using var tx = StorageManager.StartUnitOfWork();
            var seeder = new SeedDatabase();
            seeder.CreateTable(tx);
            seeder.PopulateTable(tx);

            // Commit transaction
            tx.WasSuccessful = true;
        }
    }
}
