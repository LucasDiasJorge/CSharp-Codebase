using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoUserApi.Models;

namespace MongoUserApi.Services;

public interface IUserService
{
    Task<User?> GetAsync(string id, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User> RegisterAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(string id, UserUpdateRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
