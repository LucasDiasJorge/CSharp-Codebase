using Microsoft.Data.SqlClient;
using System.Data;

namespace SafeVault.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        // Use SqlConnection which supports parameterized queries to prevent SQL injection
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
