namespace ElementSql.Interfaces
{
    //marker interface
    public interface IRepo
    {

    }

    public interface IRepository<TEntity> : IRepo where TEntity : class
    {
        Task<TEntity> GetById(object id, IConnectionContext context);
        Task<IEnumerable<TEntity>> GetAll(IConnectionContext context);
        Task<TEntity> Add(TEntity entity, IConnectionContext context);
        Task Update(TEntity entity, IConnectionContext context);
        Task Delete(TEntity entity, IConnectionContext context);
    }
}
