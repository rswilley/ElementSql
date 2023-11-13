using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    internal interface IElementRepository : IRepository<Element>
    {

    }

    internal class ElementRepository : RepositoryBase<Element>, IElementRepository
    {
        
    }

    [Table(TableConstants.Elements)]
    internal class Element
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
    }

    internal static class TableConstants
    {
        public const string Elements = "elements";
    }
}
