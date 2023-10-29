using System.Data;

namespace ElementSql.Interfaces
{
    internal interface ISessionContext : IConnectionContext
    {
        IDbConnection Connection { get; }
    }
}
