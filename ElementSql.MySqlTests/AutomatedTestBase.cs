using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
    }
}
