using ElementSql;
using ElementSql.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElementSql(this IServiceCollection services, Action<ElementSqlConfiguration> configuration)
    {
        var serviceConfig = new ElementSqlConfiguration();

        configuration.Invoke(serviceConfig);

        return services.AddElementSql(serviceConfig);
    }

    public static IServiceCollection AddElementSql(this IServiceCollection services, ElementSqlConfiguration configuration)
    {
        if (!configuration.Databases.Any())
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
