using ElementSql.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace ElementSql.MySql
{
    //public class MySqlConnectionSession : IConnectionSession
    //{
    //    public MySqlConnectionSession(string connectionString)
    //    {
    //        if (string.IsNullOrEmpty(connectionString))
    //        {
    //            throw new ArgumentNullException($"No connection string found for {nameof(MySqlConnectionSession)}.");
    //        }
    //        _connectionString = connectionString;
    //    }

    //    public async Task BeginSessionAsync()
    //    {
    //        try
    //        {
    //            _connection = new MySqlConnection(_connectionString);
    //            await _connection.OpenAsync();
    //        }
    //        catch (Exception)
    //        {
    //            _connection.Close();
    //            _connection.Dispose();

    //            throw;
    //        }
    //    }

    //    public void EndSession()
    //    {
    //        _connection.Close();
    //        _connection.Dispose();
    //    }

    //    public IDbConnection Connection => _connection;

    //    private readonly string _connectionString;
    //    private MySqlConnection _connection = null!;
    //}
}
