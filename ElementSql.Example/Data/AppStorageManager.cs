using ElementSql.Interfaces;

namespace ElementSql.Example.Data
{
    public interface IAppStorageManager : IStorageManager
    {
        IDbContext DbContext { get; set; }
    }

    public class AppStorageManager : StorageManager, IAppStorageManager
    {
        public IDbContext DbContext { get; set; }

        public AppStorageManager(IServiceProvider serviceProvider) : base(serviceProvider) 
        {
            DbContext = new DbContext(serviceProvider);
        }
    }
}
