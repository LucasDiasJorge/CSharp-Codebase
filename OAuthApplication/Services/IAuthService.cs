namespace OAuthApplication.Services;

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
    Task<bool> LogoutAsync(string refreshToken);
}
