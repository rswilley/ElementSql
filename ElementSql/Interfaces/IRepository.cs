namespace ElementSql.Interfaces
{
    //marker interface
    public interface ISqlRepository
    {

    }

    public interface IRepository<TEntity> : ISqlRepository where TEntity : class
    {
        Task<TEntity> GetById(object id, IConnectionContext context);
        Task<IEnumerable<TEntity>> GetAll(IConnectionContext context);
        Task<TEntity> Add(TEntity entity, IConnectionContext context);
        Task Update(TEntity entity, IConnectionContext context);
        Task Delete(TEntity entity, IConnectionContext context);
    }
}
