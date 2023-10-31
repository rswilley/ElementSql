using ElementSql;

namespace Microsoft.Extensions.DependencyInjection;

public class ElementSqlConfiguration
{
    public List<SqlDatabase> Databases { get; set; } = new List<SqlDatabase>();
}
