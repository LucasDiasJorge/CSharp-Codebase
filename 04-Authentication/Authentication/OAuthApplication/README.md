# OAuth Application - Autenticação Externa

## Visão geral

Projeto didático do CSharp-101 dedicado a OAuth Application - Autenticação Externa, com foco em autenticação, autorização e segurança.

## Conceitos abordados

Este projeto demonstra implementação de OAuth 2.0 para autenticação externa:

- **OAuth 2.0**: Protocolo de autorização
- **OpenID Connect**: Camada de identidade sobre OAuth
- **External Authentication**: Autenticação via provedores externos
- **JWT Tokens**: Tokens de acesso e identidade
- **Claims**: Informações do usuário
- **Social Login**: Login via Google, Facebook, GitHub, etc.
- **PKCE**: Proof Key for Code Exchange

## Objetivos de aprendizagem

- Configurar OAuth com provedores externos
- Implementar fluxo de autorização
- Gerenciar tokens de acesso e refresh
- Mapear claims de usuários
- Implementar logout seguro
- Lidar com diferentes provedores OAuth

## Estrutura do projeto

```text
OAuthApplication/
+-- Properties/
|   \-- launchSettings.json
+-- Services/
|   +-- AuthService.cs
|   +-- IAuthService.cs
|   +-- ITokenValidationService.cs
|   \-- TokenValidationService.cs
+-- .gitignore
+-- appsettings.Development.json
+-- appsettings.json
+-- AuthController.cs
+-- OAuthApplication.csproj
+-- OAuthApplication.csproj.user
\-- ...
```

## Como executar

```bash
dotnet run --project 04-Authentication/Authentication/OAuthApplication/OAuthApplication.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### OAuth Flow

1. **Authorization Request**: Redireciona para provedor
2. **User Consent**: Usuário autoriza aplicação
3. **Authorization Code**: Provedor retorna código
4. **Token Exchange**: Troca código por tokens
5. **Resource Access**: Usa tokens para acessar recursos

##### Configuration

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.Scope.Add("profile");
    options.Scope.Add("email");
});
```

##### 1. Configurar OAuth Apps

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "GitHub": {
      "ClientId": "your-github-client-id",
      "ClientSecret": "your-github-client-secret"
    }
  }
}
```

##### Google OAuth

```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
        options.SaveTokens = true;
        
        options.Events.OnCreatingTicket = context =>
        {
            var user = context.Principal;
            // Process user claims
            return Task.CompletedTask;
        };
    });
```

##### GitHub OAuth

```csharp
builder.Services.AddAuthentication()
    .AddGitHub(options =>
    {
        options.ClientId = configuration["Authentication:GitHub:ClientId"];
        options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"];
        options.Scope.Add("user:email");
        
        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    });
```

##### Microsoft OAuth

```csharp
builder.Services.AddAuthentication()
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = configuration["Authentication:Microsoft:ClientId"];
        options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
        options.Scope.Add("https://graph.microsoft.com/user.read");
    });
```

##### Authentication Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("login/{provider}")]
    public IActionResult Login(string provider, string returnUrl = "/")
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback)),
            Items = { { "returnUrl", returnUrl } }
        };
        
        return Challenge(properties, provider);
    }
    
    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var result = await HttpContext.AuthenticateAsync();
        
        if (!result.Succeeded)
            return BadRequest("Authentication failed");
            
        var claims = result.Principal.Claims.Select(c => new
        {
            Type = c.Type,
            Value = c.Value
        });
        
        return Ok(new
        {
            IsAuthenticated = result.Succeeded,
            Claims = claims,
            AuthenticationType = result.Principal.Identity.AuthenticationType
        });
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Ok(new { Message = "Logged out successfully" });
    }
}
```

##### User Profile

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProfile()
    {
        var user = HttpContext.User;
        
        return Ok(new
        {
            Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Name = user.FindFirst(ClaimTypes.Name)?.Value,
            Email = user.FindFirst(ClaimTypes.Email)?.Value,
            Picture = user.FindFirst("picture")?.Value,
            Provider = user.FindFirst("provider")?.Value
        });
    }
    
    [HttpGet("claims")]
    public IActionResult GetClaims()
    {
        return Ok(HttpContext.User.Claims.Select(c => new
        {
            Type = c.Type,
            Value = c.Value
        }));
    }
}
```

##### Custom Claims Transformation

```csharp
public class CustomClaimsTransformation : IClaimsTransformation
{
    private readonly IUserService _userService;
    
    public CustomClaimsTransformation(IUserService userService)
    {
        _userService = userService;
    }
    
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity.IsAuthenticated)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // Add custom claims from database
            var user = await _userService.GetByExternalIdAsync(userId);
            if (user != null)
            {
                identity.AddClaim(new Claim("internal_user_id", user.Id.ToString()));
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
            }
        }
        
        return principal;
    }
}

// Registration
builder.Services.AddScoped<IClaimsTransformation, CustomClaimsTransformation>();
```

##### Token Management

```csharp
public class TokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public async Task<string> GetAccessTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        return await context.GetTokenAsync("access_token");
    }
    
    public async Task<string> GetRefreshTokenAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        return await context.GetTokenAsync("refresh_token");
    }
    
    public async Task RefreshTokenAsync()
    {
        // Implement token refresh logic
        var refreshToken = await GetRefreshTokenAsync();
        // Call provider's token refresh endpoint
    }
}
```

##### User Persistence

```csharp
public class OAuthUserService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<User> CreateOrUpdateUserAsync(ClaimsPrincipal principal)
    {
        var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var provider = principal.FindFirst("provider")?.Value;
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.Provider == provider);
        
        if (user == null)
        {
            user = new User
            {
                ExternalId = externalId,
                Provider = provider,
                Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                Name = principal.FindFirst(ClaimTypes.Name)?.Value,
                Picture = principal.FindFirst("picture")?.Value,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
        }
        else
        {
            // Update existing user
            user.Name = principal.FindFirst(ClaimTypes.Name)?.Value;
            user.Email = principal.FindFirst(ClaimTypes.Email)?.Value;
            user.Picture = principal.FindFirst("picture")?.Value;
            user.LastLoginAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return user;
    }
}
```

##### Security

```csharp
// ✅ Always use HTTPS in production
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 443;
});

// ✅ Secure cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
});
```

##### Error Handling

```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.Events.OnRemoteFailure = context =>
        {
            var errorMessage = context.Failure?.Message ?? "Authentication failed";
            context.Response.Redirect($"/error?message={Uri.EscapeDataString(errorMessage)}");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });
```

##### State Management

```csharp
// ✅ Use state parameter for CSRF protection
[HttpGet("login/{provider}")]
public IActionResult Login(string provider, string returnUrl = "/")
{
    var state = Guid.NewGuid().ToString();
    HttpContext.Session.SetString("oauth_state", state);
    
    var properties = new AuthenticationProperties
    {
        RedirectUri = Url.Action(nameof(Callback)),
        Items = { 
            { "returnUrl", returnUrl },
            { "state", state }
        }
    };
    
    return Challenge(properties, provider);
}
```

## Referências

- [OAuth 2.0 RFC](https://tools.ietf.org/html/rfc6749)
- [OpenID Connect](https://openid.net/connect/)
- [ASP.NET Core External Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/)
