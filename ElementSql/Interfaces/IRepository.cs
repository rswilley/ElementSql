namespace ElementSql.Interfaces
{
    public interface IRepository<TEntity> where TEntity : EntityBase
    {
        Task<TEntity> GetById(long id, IConnectionContext context);
        Task<TEntity> GetById(int id, IConnectionContext context);
        Task<IEnumerable<TEntity>> GetAll(IConnectionContext context);
        Task<TEntity> Add(TEntity entity, IConnectionContext context);
        Task Update(TEntity entity, IConnectionContext context);
        Task Delete(TEntity entity, IConnectionContext context);
    }
}
