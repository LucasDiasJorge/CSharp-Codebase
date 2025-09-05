using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using MongoUserApi.Models;
using MongoUserApi.Repositories;

namespace MongoUserApi.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ITokenService _tokenService;

    public UserService(IUserRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<User?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        User? user = await _repository.GetByIdAsync(id, cancellationToken);
        return user;
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<User> users = await _repository.GetAllAsync(cancellationToken);
        return users;
    }

    public async Task<User> RegisterAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
    {
        User existing = await _repository.GetByEmailAsync(request.Email, cancellationToken) ?? new User();
        if (!string.IsNullOrEmpty(existing.Id))
        {
            throw new InvalidOperationException("Email already registered");
        }

        User user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            DisplayName = request.DisplayName,
            PasswordHash = HashPassword(request.Password),
            Active = true,
            CreatedAt = DateTime.UtcNow,
            Role = "user"
        };

        User created = await _repository.CreateAsync(user, cancellationToken);
        return created;
    }

    public async Task<AuthResponse?> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _repository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (user == null)
        {
            return null;
        }
        string hash = HashPassword(request.Password);
        if (hash != user.PasswordHash)
        {
            return null;
        }
        AuthResponse token = _tokenService.GenerateToken(user);
        return token;
    }

    public async Task<bool> UpdateAsync(string id, UserUpdateRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.DisplayName = request.DisplayName;
        user.Active = request.Active;
        user.Role = request.Role;

        bool updated = await _repository.UpdateAsync(user, cancellationToken);
        return updated;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        bool deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted;
    }

    private static string HashPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha.ComputeHash(bytes);
        string result = Convert.ToHexString(hash);
        return result;
    }
}
