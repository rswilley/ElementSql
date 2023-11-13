using System.Data;

namespace Microsoft.Extensions.DependencyInjection;

public class ElementSqlConfiguration
{
    public Dictionary<string, Func<IDbConnection>> Databases { get; set; } = new Dictionary<string, Func<IDbConnection>>();
    public ElementSqlRegistration Registration { get; set; } = new();
}

public class ElementSqlRegistration
{
    public bool Autoregister { get; set; } = false;
    public Type AssemblyLocation { get; set; } = null!;
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;
}