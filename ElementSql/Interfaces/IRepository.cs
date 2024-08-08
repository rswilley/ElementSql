namespace ElementSql.Interfaces
{
    /// <summary>
    /// Marker interface for IRepository
    /// </summary>
    public interface IElementSqlRepository
    { }

    public interface IRepository<TEntity> : IElementSqlRepository where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(object id, IConnectionContext context, int? commandTimeout = null);
        Task<IEnumerable<TEntity>> GetAllAsync(IConnectionContext context, int? commandTimeout = null);
        Task<TEntity> InsertAsync(TEntity entity, IConnectionContext context, int? commandTimeout = null);
        Task UpdateAsync(TEntity entity, IConnectionContext context, int? commandTimeout = null);
        Task DeleteAsync(int key, IConnectionContext context, int? commandTimeout = null);
        Task DeleteAsync(long key, IConnectionContext context, int? commandTimeout = null);
        Task DeleteAsync(Guid key, IConnectionContext context, int? commandTimeout = null);

        TEntity? GetById(object id, IConnectionContext context, int? commandTimeout = null);
        IEnumerable<TEntity> GetAll(IConnectionContext context, int? commandTimeout = null);
        TEntity Insert(TEntity entity, IConnectionContext context, int? commandTimeout = null);
        void Update(TEntity entity, IConnectionContext context, int? commandTimeout = null);
        void Delete(int key, IConnectionContext context, int? commandTimeout = null);
        void Delete(long key, IConnectionContext context, int? commandTimeout = null);
        void Delete(Guid key, IConnectionContext context, int? commandTimeout = null);
    }
}
