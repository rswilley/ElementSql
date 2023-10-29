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
        if (configuration.ConnectionSession == null)
        {
            throw new ArgumentNullException("No ConnectionSession setup.");
        }
        if (configuration.UnitOfWork == null)
        {
            throw new ArgumentNullException("No UnitOfWork setup.");
        }

        services.Add(new ServiceDescriptor(typeof(IConnectionSession), sp => configuration.ConnectionSession, ServiceLifetime.Transient));
        services.Add(new ServiceDescriptor(typeof(IUnitOfWork), sp => configuration.UnitOfWork, ServiceLifetime.Transient));

        return services;
    }
}
