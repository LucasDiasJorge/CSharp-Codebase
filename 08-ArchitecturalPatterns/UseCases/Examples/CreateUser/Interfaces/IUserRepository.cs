using UseCases.Examples.CreateUser.Entities;

namespace UseCases.Examples.CreateUser.Interfaces;

/// <summary>
/// Interface do repositório de usuários (Port)
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
}
