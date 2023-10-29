using ElementSql.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public class ElementSqlConfiguration
{
    public IConnectionSession ConnectionSession { get; set; } = null!;
    public IUnitOfWork UnitOfWork { get; set; } = null!;
}
