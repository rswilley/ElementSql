using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Testcontainers.MySql;

namespace ElementSql.MySqlTests
{
    public abstract class AutomatedTestBase : IAsyncDisposable
    {
        protected AutomatedTestBase()
        {
            var connectionString = InitializeDatabase().Result;
            BootstrapDependencies(connectionString);
            SeedTestDatabase();
        }

        protected IServiceProvider ServiceProvider { get; private set; } = null!;
        protected IMyStorageManager StorageManager { get; private set; } = null!;
        protected IElementRepository ElementRepository { get; private set; } = null!;

        protected async Task<Element> ShouldCreateRecord(Element toInsert, IConnectionContext context)
        {
            var record = await ElementRepository.InsertAsync(toInsert, context);
            return record;
        }

        protected async Task<Element?> ShouldReadRecord(int recordId, IConnectionContext context)
        {
            var record = await ElementRepository.GetByIdAsync(recordId, context);
            return record;
        }

        protected async Task<Element?> ShouldUpdateRecord(Element recordToUpdate, IConnectionContext context)
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

        protected async Task<Element?> ShouldDeleteRecord(Element recordToDelete, IConnectionContext context)
        {
            await ElementRepository.DeleteAsync(recordToDelete.Id, context);
            var deleted = await ElementRepository.GetByIdAsync(recordToDelete.Id, context);
            return deleted;
        }
        
        private async Task<string> InitializeDatabase()
        {
            // Create a MySQL container configuration
            _mySqlContainer = new MySqlBuilder()
                .WithDatabase("TestDb")
                .WithUsername("testuser")
                .WithPassword("testpass")
                .WithImage("mysql:8.0")
                .Build();

            await _mySqlContainer.StartAsync();
                
            Console.WriteLine("MySQL container started.");
            Console.WriteLine($"Connection string: {_mySqlContainer.GetConnectionString()}");
            
            return _mySqlContainer.GetConnectionString();
        }
        
        private MySqlContainer? _mySqlContainer;

        private void BootstrapDependencies(string connectionString)
        {
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddSingleton<IMyStorageManager, MyStorageManager>();
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
            StorageManager = ServiceProvider.GetRequiredService<IMyStorageManager>();
            ElementRepository = StorageManager.DbContext.ElementRepository;

            using var tx = StorageManager.StartUnitOfWork();
            var seeder = new SeedDatabase();
            seeder.CreateTable(tx);
            seeder.PopulateTable(tx);

            // Commit transaction
            tx.WasSuccessful = true;
        }

        public async ValueTask DisposeAsync()
        {
            await _mySqlContainer!.DisposeAsync();
            Console.WriteLine("MySQL container stopped.");
        }
    }
}
