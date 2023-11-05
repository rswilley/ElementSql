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
        }

        protected IServiceProvider ServiceProvider { get; private set; } = null!;

        private void Bootstrap()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IElementRepository, ElementRepository>();
            serviceCollection.AddSingleton<ITestQuery, TestQuery>();

            var connectionString = Environment.GetEnvironmentVariable("MySqlConnectionString")!;
            serviceCollection.AddElementSql(config =>
            {
                config.Databases.Add("Default", () => new MySqlConnection(connectionString));
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
