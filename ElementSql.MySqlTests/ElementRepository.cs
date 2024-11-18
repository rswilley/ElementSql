using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Interfaces;

namespace ElementSql.MySqlTests
{
    public interface IElementRepository : IRepository<Element>
    {
        Task<Element?> GetByName(string name, IConnectionContext context);
    }

    public class ElementRepository : RepositoryBase<Element, int>, IElementRepository
    {
        public async Task<Element?> GetByName(string name, IConnectionContext context)
        {
            return await QuerySingleOrDefaultAsync($"WHERE {nameof(Element.Name)} = @Name", new { name }, context);
        }
    }

    [Table("elements")]
    public class Element : EntityBase<int>
    {
        [Key]
        public override int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Symbol { get; set; } = null!;
    }
}
