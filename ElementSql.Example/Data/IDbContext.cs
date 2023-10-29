using ElementSql.Example.Data.PersonRepository;

namespace ElementSql.Example.Data
{
    public interface IDbContext
    {
        IPersonRepository PersonRepository { get; }
    }

    public class DbContext : IDbContext
    {
        public DbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPersonRepository PersonRepository
        {
            get
            {
                _personRepository ??= _serviceProvider.GetRequiredService<IPersonRepository>();
                return _personRepository;
            }
        }

        private readonly IServiceProvider _serviceProvider;

        private IPersonRepository _personRepository = null!;
    }
}
