using System.Data;

namespace ElementSql.Interfaces
{
    internal interface IUnitOfWorkContext : IConnectionContext
    {
        IDbTransaction Transaction { get; }
    }
}
