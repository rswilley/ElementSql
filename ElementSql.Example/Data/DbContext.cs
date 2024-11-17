using ElementSql.Example.Data.PersonRepository;

namespace ElementSql.Example.Data
{
    public interface IDbContext
    {
        IPersonRepository PersonRepository { get;}
    }

    public class DbContext : IDbContext
    {
        public IPersonRepository PersonRepository => _personRepository ??= _serviceProvider.GetRequiredService<IPersonRepository>();

        public DbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private IPersonRepository _personRepository;
    }
}
