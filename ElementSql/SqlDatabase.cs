using ElementSql.Interfaces;

namespace ElementSql
{
    public class SqlDatabase : ISqlDatabase
    {
        public IConnectionSession Session { get; set; } = null!;

        public IUnitOfWork UnitOfWork { get; set; } = null!;
        public string Name { get; set; } = "Default";
    }
}
