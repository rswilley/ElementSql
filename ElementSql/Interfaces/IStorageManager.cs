namespace ElementSql.Interfaces
{
    public interface IStorageManager
    {
        Task<IConnectionContext> StartSession();
        Task<IConnectionContext> StartUnitOfWork();
    }
}
