using ElementSql;
using ElementSql.Interfaces;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddElementSql(Action<ElementSqlConfiguration> configuration)
        {
            var serviceConfig = new ElementSqlConfiguration();

            configuration.Invoke(serviceConfig);

            return services.AddElementSql(serviceConfig);
        }

        private IServiceCollection AddElementSql(ElementSqlConfiguration configuration)
        {
            if (configuration.Databases.Count == 0)
            {
                throw new ArgumentNullException("No Databases setup.");
            }

            foreach (var database in configuration.Databases)
            {
                if (database.Value == null)
                {
                    throw new ArgumentNullException($"No DbConnection provided for database {database.Key}.");
                }

                services.AddTransient<ISqlDatabase>(sp => new SqlDatabase
                {
                    Name = database.Key,
                    DbConnection = database.Value
                });
            }

            return services;
        }
    }
}
