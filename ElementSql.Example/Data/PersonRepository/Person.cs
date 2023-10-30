using Dapper.Contrib.Extensions;

namespace ElementSql.Example.Data.PersonRepository
{
    [Table(TableConstants.Person)]
    public class Person
    {
        public int Id { get; set;}
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public Guid UniqueId { get; set; } = Guid.NewGuid();
    }
}
