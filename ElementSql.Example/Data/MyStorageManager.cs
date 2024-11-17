using ElementSql.Interfaces;

namespace ElementSql.Example.Data
{
    public interface IMyStorageManager : IStorageManager
    {
        IDbContext DbContext { get; set; }
    }

    public class MyStorageManager : StorageManager, IMyStorageManager
    {
        private readonly IServiceProvider _serviceProvider;

        public MyStorageManager(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            DbContext = new DbContext(serviceProvider);
        }

        public IDbContext DbContext { get; set; }
    }
}
