﻿using ElementSql.Interfaces;
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
            serviceCollection.AddSingleton<ITestQuery, TestQuery>();
            serviceCollection.AddTransient<IStorageManager, StorageManager>();

            var connectionString = Environment.GetEnvironmentVariable("MySqlConnectionString")!;
            serviceCollection.AddElementSql(config =>
            {
                config.Databases.Add(new SqlDatabase
                {
                    Name = "Default",
                    Session = new MySqlConnectionSession(connectionString),
                    UnitOfWork = new MySqlUnitOfWork(connectionString)
                });
            });

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
