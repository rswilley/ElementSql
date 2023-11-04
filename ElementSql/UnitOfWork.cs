using ElementSql.Interfaces;
using System.Data;
using System.Data.Common;

namespace ElementSql
{
    internal class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IDbConnection dbConnection)
        {
            _connection = dbConnection ?? throw new ArgumentNullException($"No DbConnection provided.");
        }

        public async Task BeginTransactionAsync(IsolationLevel il = IsolationLevel.Unspecified)
        {
            try
            {
                if (_connection is DbConnection connection)
                {
                    await connection.OpenAsync();
                }
                else
                {
                    throw new InvalidOperationException("Async operations require use of a DbConnection or an already-open IDbConnection");
                }

                _transaction = _connection.BeginTransaction(il);
            }
            catch (Exception)
            {
                _transaction.Dispose();
                _connection.Close();
                _connection.Dispose();
                throw;
            }
        }

        public void BeginTransaction(IsolationLevel il = IsolationLevel.Unspecified)
        {
            try
            {
                _connection.Open();
                _transaction = _connection.BeginTransaction(il);
            } catch (Exception)
            {
                _transaction.Dispose();
                _connection.Close();
                _connection.Dispose();
                throw;
            }
        }

        public void Commit()
        {
            _transaction.Commit();
            _connection.Close();
            _transaction.Dispose();
            _connection.Dispose();
        }

        public void RollBack()
        {
            _transaction.Rollback();
            _connection.Close();
            _transaction.Dispose();
            _connection.Dispose();
        }

        public IDbTransaction GetTransaction()
        {
            return _transaction;
        }

        public IDbConnection GetConnection()
        {
            return _connection;
        }

        private readonly IDbConnection _connection = null!;
        private IDbTransaction _transaction = null!;
    }
}
