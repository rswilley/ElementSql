using ElementSql.Interfaces;
using System.Data;
using System.Data.Common;

namespace ElementSql
{
    internal class ConnectionSession : IConnectionSession
    {
        public ConnectionSession(IDbConnection dbConnection)
        {
            _connection = dbConnection ?? throw new ArgumentNullException($"No DbConnection provided.");
        }

        public async Task BeginSessionAsync()
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
            }
            catch (Exception)
            {
                _connection.Close();
                _connection.Dispose();

                throw;
            }
        }

        public void BeginSession()
        {
            try
            {
                _connection.Open();
            }
            catch (Exception)
            {
                _connection.Close();
                _connection.Dispose();

                throw;
            }
        }

        public void EndSession()
        {
            _connection.Close();
            _connection.Dispose();
        }

        public IDbConnection Connection => _connection;

        private readonly IDbConnection _connection = null!;
    }
}
