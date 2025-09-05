using MongoUserApi.Models;

namespace MongoUserApi.Services;

public interface ITokenService
{
    AuthResponse GenerateToken(User user);
}
