using ElementSql.Interfaces;

namespace ElementSql.Example.Data.PersonRepository
{
    public interface IPersonRepository : IRepository<Person>
    {
        Task<Person?> GetByEmailAddress(string emailAddress, IConnectionContext context);
    }

    public class PersonRepository : RepositoryBase<Person, long>, IPersonRepository
    {
        public async Task<Person?> GetByEmailAddress(string emailAddress, IConnectionContext context)
        {
            return await QuerySingleOrDefaultAsync($"WHERE {nameof(Person.EmailAddress)} = @EmailAddress",
                new
                {
                    emailAddress
                }, context);
        }
    }
}
