namespace OAuthApplication.Services;

using System.Threading.Tasks;

public interface ITokenValidationService
{
    Task<bool> ValidateTokenAsync(string token);
}
