namespace OAuthApplication;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(string username, string password)
    {
        var keycloakConfig = _configuration.GetSection("Keycloak");
        var tokenEndpoint = keycloakConfig["TokenEndpoint"];
        var clientId = keycloakConfig["ClientId"];
        var clientSecret = keycloakConfig["ClientSecret"];

        var requestData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "username", username },
            { "password", password },
            { "scope", "openid profile email" }
        };

        var requestContent = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync(tokenEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
            return null;

        var responseContent = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseContent);
        return jsonDoc.RootElement.GetProperty("access_token").GetString();
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var keycloakConfig = _configuration.GetSection("Keycloak");
        var logoutEndpoint = keycloakConfig["LogoutEndpoint"];
        var clientId = keycloakConfig["ClientId"];
        var clientSecret = keycloakConfig["ClientSecret"];

        var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "refresh_token", refreshToken }
        };

        var requestContent = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync(logoutEndpoint, requestContent);

        return response.IsSuccessStatusCode;
    }
}
