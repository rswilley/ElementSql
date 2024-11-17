using Microsoft.Extensions.DependencyInjection;

namespace ElementSql.MySqlTests
{
    public interface IDbContext
    {
        IElementRepository ElementRepository { get; }
        ITestQuery TestQuery { get; }
    }

    public class DbContext : IDbContext
    {
        public IElementRepository ElementRepository => _elementRepository ??= _serviceProvider.GetRequiredService<IElementRepository>();
        public ITestQuery TestQuery => _testQuery ??= _serviceProvider.GetRequiredService<ITestQuery>();

        public DbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private IElementRepository _elementRepository;
        private ITestQuery _testQuery;
    }
}
