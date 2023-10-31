namespace ElementSql.Interfaces
{
    //marker interface
    public interface ISqlRepository
    {

    }

    // GetAsync
    // GetAllAsync
    // InsertAsync
    // UpdateAsync
    // DeleteAsync
    // DeleteAllAsync

    public interface IRepository<TEntity> : ISqlRepository where TEntity : class
    {
        Task<TEntity> GetById(object id, IConnectionContext context, int? commandTimeout = null);
        Task<IEnumerable<TEntity>> GetAll(IConnectionContext context, int? commandTimeout = null);
        Task<TEntity> Add(TEntity entity, IConnectionContext context, int? commandTimeout = null);
        Task Update(TEntity entity, IConnectionContext context, int? commandTimeout = null);
        Task<bool> Delete(TEntity entity, IConnectionContext context, int? commandTimeout = null);
    }
}
