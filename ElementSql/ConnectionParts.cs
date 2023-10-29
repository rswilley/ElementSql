using System.Data;

namespace ElementSql
{
    public class ConnectionParts
    {
        public IDbConnection Connection { get; set; } = null!;
        public IDbTransaction? Transaction { get; set; }
    }
}
