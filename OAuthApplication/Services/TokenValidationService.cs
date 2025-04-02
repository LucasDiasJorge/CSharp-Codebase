namespace OAuthApplication.Services;

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

public class TokenValidationService : ITokenValidationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TokenValidationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var keycloakConfig = _configuration.GetSection("Keycloak");
        var introspectionEndpoint = $"{keycloakConfig["TokenEndpoint"]}/introspect";
        var clientId = keycloakConfig["ClientId"];
        var clientSecret = keycloakConfig["ClientSecret"];

        var requestData = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "token", token }
        };

        var requestContent = new FormUrlEncodedContent(requestData);
        var response = await _httpClient.PostAsync(introspectionEndpoint, requestContent);

        if (!response.IsSuccessStatusCode)
            return false;

        var responseContent = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseContent);

        return jsonDoc.RootElement.TryGetProperty("active", out var isActive) && isActive.GetBoolean();
    }
}
