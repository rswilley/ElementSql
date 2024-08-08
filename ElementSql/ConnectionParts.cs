using System.Data;

namespace ElementSql
{
    public class ConnectionParts
    {
        public IDbConnection Connection { get; internal set; } = null!;
        public IDbTransaction? Transaction { get; internal set; }
    }
}
