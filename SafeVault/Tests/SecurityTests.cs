using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SafeVault.Models.DTOs;

namespace SafeVault.Tests;

/// <summary>
/// Security tests for the SafeVault API
/// </summary>
public class SecurityTests
{
    private readonly HttpClient _client;
    
    public SecurityTests()
    {
        // Create a test web application factory
        var factory = new WebApplicationFactory<Program>();
        _client = factory.CreateClient();
    }
    
    /// <summary>
    /// Test SQL injection prevention in login endpoint
    /// </summary>
    [Fact]
    public async Task Login_WithSqlInjection_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto
        {
            // SQL injection attempt
            EmailOrUsername = "' OR 1=1 --",
            Password = "anything"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/login", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    /// <summary>
    /// Test XSS prevention in secret creation
    /// </summary>
    [Fact]
    public async Task CreateSecret_WithXssPayload_SanitizesContent()
    {
        // Arrange
        // First login to get a token
        var loginDto = new UserLoginDto
        {
            EmailOrUsername = "admin@safevault.com",
            Password = "Admin@123456"
        };
        
        var loginContent = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeAnonymousType(loginResult, new { token = "" })?.token;
        
        // Create secret with XSS payload
        var secretDto = new SecretCreateDto
        {
            Title = "Test Secret",
            Content = "<script>alert('XSS');</script>This is a test secret"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(secretDto), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.PostAsync("/api/secret", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        // Verify the content was sanitized by retrieving all secrets
        var secretsResponse = await _client.GetAsync("/api/secret");
        var secretsContent = await secretsResponse.Content.ReadAsStringAsync();
        var secrets = JsonConvert.DeserializeAnonymousType(secretsContent, 
            new[] { new { id = 0, title = "", content = "" } });
        
        var createdSecret = secrets?.FirstOrDefault(s => s.title == "Test Secret");
        
        // Content should be sanitized - script tags should be encoded
        Assert.Contains("&lt;script&gt;", createdSecret?.content);
        Assert.DoesNotContain("<script>", createdSecret?.content);
    }
    
    /// <summary>
    /// Test authorization - accessing restricted endpoint
    /// </summary>
    [Fact]
    public async Task SecretEndpoint_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange - don't set any authentication
        
        // Act
        var response = await _client.GetAsync("/api/secret");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    /// <summary>
    /// Test password complexity requirements
    /// </summary>
    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var registrationDto = new UserRegistrationDto
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "password", // Too simple
            ConfirmPassword = "password"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(registrationDto), Encoding.UTF8, "application/json");
        
        // Act
        var response = await _client.PostAsync("/api/auth/register", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    /// <summary>
    /// Test RBAC - regular user accessing admin endpoint
    /// </summary>
    [Fact]
    public async Task AssignRole_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        // First register a regular user
        var registrationDto = new UserRegistrationDto
        {
            Email = "regular@example.com",
            Username = "regularuser",
            Password = "SecureP@ssw0rd123",
            ConfirmPassword = "SecureP@ssw0rd123"
        };
        
        var registerContent = new StringContent(JsonConvert.SerializeObject(registrationDto), Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/auth/register", registerContent);
        
        // Login as regular user
        var loginDto = new UserLoginDto
        {
            EmailOrUsername = "regular@example.com",
            Password = "SecureP@ssw0rd123"
        };
        
        var loginContent = new StringContent(JsonConvert.SerializeObject(loginDto), Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
        var loginResult = await loginResponse.Content.ReadAsStringAsync();
        var token = JsonConvert.DeserializeAnonymousType(loginResult, new { token = "" })?.token;
        
        // Try to assign a role (admin-only endpoint)
        var assignRoleDto = new AssignRoleDto
        {
            UserId = 1,
            Role = "Admin"
        };
        
        var content = new StringContent(JsonConvert.SerializeObject(assignRoleDto), Encoding.UTF8, "application/json");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Act
        var response = await _client.PutAsync("/api/auth/assign-role", content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
