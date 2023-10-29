using ElementSql.Interfaces;
using ElementSql.MySql;
using Microsoft.Extensions.DependencyInjection;

namespace ElementSql.MySqlTests
{
    public abstract class AutomatedTestBase
    {
        protected AutomatedTestBase()
        {
            Bootstrap();
        }

        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        private void Bootstrap()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ITestRepository, TestRepository>();
            serviceCollection.AddTransient<IStorageManager, StorageManager>();

            var connectionString = "server=localhost;uid=element;pwd=password;database=element";
            serviceCollection.AddElementSql(config =>
            {
                config.ConnectionSession = new MySqlConnectionSession(connectionString);
                config.UnitOfWork = new MySqlUnitOfWork(connectionString);
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
