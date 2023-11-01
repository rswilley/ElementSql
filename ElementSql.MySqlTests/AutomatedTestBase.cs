﻿using ElementSql.Interfaces;
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

            serviceCollection.AddSingleton<ITestRepository, TestRepository>();
            serviceCollection.AddSingleton<ITestQuery, TestQuery>();
            serviceCollection.AddTransient<IStorageManager, StorageManager>();

            var connectionString = Environment.GetEnvironmentVariable("MySqlConnectionString")!;
            serviceCollection.AddElementSql(config =>
            {
                config.Databases.Add(new SqlDatabase
                {
                    Name = "Default",
                    DbConnection = () => new MySqlConnection(connectionString)
                });
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
