using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    public interface IElementRepository : IRepository<Element>
    {
        Task<Element?> GetByName(string name, IConnectionContext context);
    }

    public class ElementRepository : RepositoryBase<Element>, IElementRepository
    {
        public async Task<Element?> GetByName(string name, IConnectionContext context)
        {
            return await QueryFirstOrDefaultAsync($"WHERE {nameof(Element.Name)} = @Name", new { name }, context);
        }
    }

    [Table("elements")]
    public class Element
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
    }
}
