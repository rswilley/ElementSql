using System.ComponentModel.DataAnnotations;
using ElementSql.Attributes;
using ElementSql.Interfaces;

namespace ElementSql.Example.Data
{
    [Table(TableConstants.Person)]
    public record Person : EntityBase<ulong>
    {
        [Key]
        public override ulong Id { get; init; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public Guid UniqueId { get; set; } = Guid.NewGuid();
    }
}
