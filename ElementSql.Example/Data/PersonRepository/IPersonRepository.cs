using ElementSql.Interfaces;

namespace ElementSql.Example.Data.PersonRepository
{
    public interface IPersonRepository : IRepository<Person>
    {
        Task<Person?> GetByEmailAddress(string emailAddress, IConnectionContext context);
    }

    public class PersonRepository : RepositoryBase<Person>, IPersonRepository
    {
        public async Task<Person?> GetByEmailAddress(string emailAddress, IConnectionContext context)
        {
            return await ReadSingleOrDefault<Person>($@"
                SELECT {GetColumns()} 
                FROM {Table} 
                WHERE {nameof(Person.EmailAddress)} = @EmailAddress",
                new
                {
                    emailAddress
                }, context);
        }

        internal const string Table = TableConstants.Person;
    }
}
