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

        protected IStorageManager StorageManager { get; private set; } = null!;

        private IServiceProvider ServiceProvider { get; set; } = null!;

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

            serviceCollection.AddElementSql(config =>
            {
                config.Databases.Add("Default", () => new MySqlConnection(connectionString));
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private void SeedTestDatabase()
        {
            // Populate test data
            StorageManager = new StorageManager(ServiceProvider);

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
