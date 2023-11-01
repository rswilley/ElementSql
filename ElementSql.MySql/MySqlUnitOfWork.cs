using ElementSql.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace ElementSql.MySql
{
    //public class MySqlUnitOfWork : IUnitOfWork
    //{
    //    public MySqlUnitOfWork(string connectionString)
    //    {
    //        if (string.IsNullOrEmpty(connectionString))
    //        {
    //            throw new ArgumentNullException($"No connection string found for {nameof(MySqlUnitOfWork)}.");
    //        }
    //        _connectionString = connectionString;
    //    }

    //    public async Task BeginTransactionAsync()
    //    {
    //        try
    //        {
    //            _connection = new MySqlConnection(_connectionString);
    //            await _connection.OpenAsync();
    //            _transaction = _connection.BeginTransaction();
    //        }
    //        catch (Exception)
    //        {
    //            _transaction.Dispose();
    //            _connection.Close();
    //            _connection.Dispose();
    //            throw;
    //        }
    //    }

    //    public void Commit()
    //    {
    //        _transaction.Commit();
    //        _connection.Close();
    //        _transaction.Dispose();
    //        _connection.Dispose();
    //    }

    //    public void RollBack()
    //    {
    //        _transaction.Rollback();
    //        _connection.Close();
    //        _transaction.Dispose();
    //        _connection.Dispose();
    //    }

    //    public IDbTransaction GetTransaction()
    //    {
    //        return _transaction;
    //    }

    //    public IDbConnection GetConnection()
    //    {
    //        return _connection;
    //    }

    //    private readonly string _connectionString = string.Empty;
    //    private MySqlTransaction _transaction = null!;
    //    private MySqlConnection _connection = null!;
    //}
}
