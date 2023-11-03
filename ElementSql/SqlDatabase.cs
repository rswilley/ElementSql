using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    internal class SqlDatabase : ISqlDatabase
    {
        public Func<IDbConnection> DbConnection { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
