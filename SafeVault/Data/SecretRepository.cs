using Dapper;
using SafeVault.Models;
using System.Data;

namespace SafeVault.Data;

public interface ISecretRepository
{
    Task<Secret?> GetByIdAsync(int id, int userId);
    Task<IEnumerable<Secret>> GetAllByUserIdAsync(int userId);
    Task<int> CreateAsync(Secret secret);
    Task<bool> UpdateAsync(Secret secret);
    Task<bool> DeleteAsync(int id, int userId);
}

public class SecretRepository : ISecretRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SecretRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Secret?> GetByIdAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with access control (userId) to prevent SQL injection and unauthorized access
        var sql = "SELECT * FROM Secrets WHERE Id = @Id AND UserId = @UserId";
        return await connection.QuerySingleOrDefaultAsync<Secret>(sql, new { Id = id, UserId = userId });
    }

    public async Task<IEnumerable<Secret>> GetAllByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with access control (userId) to prevent SQL injection and unauthorized access
        var sql = "SELECT * FROM Secrets WHERE UserId = @UserId";
        return await connection.QueryAsync<Secret>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(Secret secret)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query for all fields to prevent SQL injection
        var sql = @"
            INSERT INTO Secrets (UserId, Title, Content, CreatedAt, UpdatedAt)
            VALUES (@UserId, @Title, @Content, @CreatedAt, @UpdatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            secret.UserId,
            secret.Title,
            secret.Content,
            secret.CreatedAt,
            secret.UpdatedAt
        });
    }

    public async Task<bool> UpdateAsync(Secret secret)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with access control (UserId) to prevent SQL injection and unauthorized access
        var sql = @"
            UPDATE Secrets 
            SET Title = @Title, 
                Content = @Content, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND UserId = @UserId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            secret.Title,
            secret.Content,
            UpdatedAt = DateTime.UtcNow,
            secret.Id,
            secret.UserId
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with access control (userId) to prevent SQL injection and unauthorized access
        var sql = "DELETE FROM Secrets WHERE Id = @Id AND UserId = @UserId";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UserId = userId });
        
        return rowsAffected > 0;
    }
}
