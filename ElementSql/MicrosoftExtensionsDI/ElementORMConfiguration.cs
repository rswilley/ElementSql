using System.Data;

namespace Microsoft.Extensions.DependencyInjection;

public class ElementSqlConfiguration
{
    public Dictionary<string, Func<IDbConnection>> Databases { get; set; } = new Dictionary<string, Func<IDbConnection>>();
}