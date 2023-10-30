using Dapper;
using Dapper.Contrib.Extensions;
using ElementSql.Interfaces;
using System.Data;

namespace ElementSql
{
    public abstract class RepositoryBase<TEntity> where TEntity : class
    {
        public async Task<TEntity> GetById(object id, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAsync<TEntity>(id, parts.Transaction);
        }

        public async Task<IEnumerable<TEntity>> GetAll(IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.GetAllAsync<TEntity>(parts.Transaction);
        }

        public async Task<TEntity> Add(TEntity entity, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            var id = await parts.Connection.InsertAsync(entity, parts.Transaction);

            var type = entity.GetType();
            var idProperty = type.GetProperty("Id");
            if (idProperty?.PropertyType == typeof(int) || idProperty?.PropertyType == typeof(long))
            {
                idProperty.SetValue(entity, id, null);
            }

            return entity;
        }

        public async Task Update(TEntity entity, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            await parts.Connection.UpdateAsync(entity, parts.Transaction);
        }

        public async Task Delete(TEntity entity, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            await parts.Connection.DeleteAsync(entity, parts.Transaction);
        }

        public async Task<TEntity?> ReadSingleOrDefault(string query, object param, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QuerySingleOrDefaultAsync<TEntity>(query, param, parts.Transaction);
        }

        public async Task<TEntity?> ReadFirstOrDefault(string query, object param, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryFirstOrDefaultAsync<TEntity>(query, param, parts.Transaction);
        }

        public async Task<IEnumerable<TEntity>> ReadMany(string query, object param, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.QueryAsync<TEntity>(query, param, parts.Transaction);
        }

        public async Task<IDataReader> ReadDataReader(string query, object param, IConnectionContext context)
        {
            var parts = context.GetConnectionParts();
            return await parts.Connection.ExecuteReaderAsync(query, param, parts.Transaction);
        }

        public string GetColumns()
        {
            return ColumnBuilder<TEntity>.GetColumns();
        }
    }
}
