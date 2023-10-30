using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    internal interface ITestRepository : IRepository<Element>
    {

    }

    internal class TestRepository : RepositoryBase<Element>, ITestRepository
    {
    }

    [Table(TableConstants.Elements)]
    internal class Element
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public Guid UniqueId { get; set; } = Guid.NewGuid();
    }

    internal static class TableConstants
    {
        public const string Elements = "elements";
    }
}
