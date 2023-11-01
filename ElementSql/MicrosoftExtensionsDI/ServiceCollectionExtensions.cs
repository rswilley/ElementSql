using ElementSql;
using ElementSql.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;

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
        if (configuration.Databases.Count > 1 && configuration.Databases.All(d => d.Name.ToLower() == "default")) {
            throw new ArgumentNullException("Only one database can be named Default.");
        }
        
        foreach (var database in configuration.Databases)
        {
            if (database.DbConnection == null)
            {
                throw new ArgumentNullException($"No DbConnection provided for database {database.Name}.");
            }

            services.AddTransient<ISqlDatabase>(sp => database);
        }

        return services;
    }
}
