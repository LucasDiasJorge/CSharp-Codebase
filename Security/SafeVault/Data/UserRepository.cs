using Dapper;
using SafeVault.Models;
using System.Data;

namespace SafeVault.Data;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> ChangePasswordAsync(int userId, string passwordHash);
    Task<bool> UpdateRoleAsync(int userId, string role);
    Task<IEnumerable<User>> GetAllAsync();
}

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query to prevent SQL injection
        var sql = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with normalized email to prevent SQL injection
        var sql = "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email.ToLower() });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with case-insensitive comparison to prevent SQL injection
        var sql = "SELECT * FROM Users WHERE LOWER(Username) = LOWER(@Username) AND IsActive = 1";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with normalized input to prevent SQL injection
        var sql = "SELECT * FROM Users WHERE (LOWER(Email) = LOWER(@Input) OR LOWER(Username) = LOWER(@Input)) AND IsActive = 1";
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Input = emailOrUsername });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query for all fields to prevent SQL injection
        var sql = @"
            INSERT INTO Users (Email, Username, PasswordHash, Role, CreatedAt, IsActive)
            VALUES (@Email, @Username, @PasswordHash, @Role, @CreatedAt, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        // All parameters are properly sanitized
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            user.Email,
            user.Username,
            user.PasswordHash,
            user.Role,
            user.CreatedAt,
            user.IsActive
        });
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query to update user, preventing SQL injection
        var sql = @"
            UPDATE Users 
            SET Email = @Email, 
                Username = @Username, 
                Role = @Role,
                IsActive = @IsActive
            WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            user.Email,
            user.Username,
            user.Role,
            user.IsActive,
            user.Id
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string passwordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query to update password, preventing SQL injection
        var sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            PasswordHash = passwordHash,
            Id = userId
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateRoleAsync(int userId, string role)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Use parameterized query with validation for role to prevent SQL injection
        var sql = "UPDATE Users SET Role = @Role WHERE Id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Role = role,
            Id = userId
        });
        
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Simple query with no parameters, but still safe from SQL injection
        var sql = "SELECT * FROM Users WHERE IsActive = 1";
        return await connection.QueryAsync<User>(sql);
    }
}
