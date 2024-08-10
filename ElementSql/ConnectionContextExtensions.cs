using ElementSql.Cache;
using ElementSql.Interfaces;

namespace ElementSql
{
    public static class ConnectionContextExtensions
    {
        public static ConnectionParts GetConnectionParts(this IConnectionContext context)
        {
            var parts = context switch
            {
                IUnitOfWorkContext unitOfWorkContext => new ConnectionParts
                {
                    Connection = unitOfWorkContext.Transaction.Connection!,
                    Transaction = unitOfWorkContext.Transaction
                },
                ISessionContext sessionContext => new ConnectionParts
                {
                    Connection = sessionContext.Connection,
                    Transaction = null
                },
                _ => throw new NotSupportedException()
            };
            
            CacheTableHelper.Initialize(parts.Connection);
            return parts;
        }
    }
}
