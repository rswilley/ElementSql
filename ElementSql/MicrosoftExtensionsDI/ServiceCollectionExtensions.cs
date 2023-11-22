using ElementSql;
using ElementSql.Interfaces;
using System.Reflection;

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

        services.AddTransient<IStorageManager, StorageManager>();

        if (configuration.Registration != null && 
            configuration.Registration.Autoregister && 
            configuration.Registration.AssemblyLocation != null)
        {
            var assembly = Assembly.GetAssembly(configuration.Registration.AssemblyLocation);
            if (assembly != null)
            {
                AutoRegister(services, assembly, "Repository", nameof(IElementSqlRepository), configuration.Registration.ServiceLifetime);
                AutoRegister(services, assembly, "Query", nameof(IElementSqlQuery), configuration.Registration.ServiceLifetime);
            } else
            {
                throw new ArgumentNullException("Repository registration assembly not found.");
            }
        }

        return services;
    }

    private static void AutoRegister(IServiceCollection services, Assembly assembly, string classNameEndsWith, string interfaceName, ServiceLifetime serviceLifetime)
    {
        var toRegister = assembly.DefinedTypes
                    .Where(t => t.IsClass && t.Name.EndsWith(classNameEndsWith))
                    .SelectMany(t => t.GetInterfaces(), (c, i) => new { Class = c, Interface = i })
                    .ToList();

        foreach (var item in toRegister)
        {
            var elementSqlRepository = toRegister.SingleOrDefault(r => r.Class.Name == item.Class.Name && r.Interface.Name == interfaceName);
            if (elementSqlRepository == null)
            {
                continue;
            }

            if ("I" + item.Class.Name == item.Interface.Name)
            {
                services.Add(new ServiceDescriptor(item.Interface, item.Class, serviceLifetime));
            }
        }
    }
}
