# CustomMiddleware - Pipeline de Middleware Personalizado

## Visão geral

**Middlewares** são componentes que formam o pipeline de processamento de requisições HTTP no ASP.NET Core. Cada middleware:

- Processa requisições HTTP de entrada
- Decide se passa a requisição para o próximo middleware
- Pode processar a resposta quando ela volta
- Implementa funcionalidades transversais (logging, autenticação, caching, etc.)

## Conceitos abordados

- Exemplo didático sobre CustomMiddleware - Pipeline de Middleware Personalizado no contexto de ASP.NET Core, contratos HTTP e pipeline web.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender o **pipeline de middleware** do ASP.NET Core
- Criar **middlewares personalizados** para funcionalidades específicas
- Implementar **logging**, **timing**, **headers customizados** e **correlation IDs**
- Organizar middlewares com **extension methods**
- Configurar **ordem de execução** dos middlewares
- Aplicar **padrões de middleware** para cross-cutting concerns

## Estrutura do projeto

```text
CustomMiddleware/
+-- Middlewares/
|   +-- Extensions/
|   +-- CorrelationIdMiddleware.cs
|   +-- CustomHeaderMiddleware.cs
|   +-- RequestResponseLoggingMiddleware.cs
|   \-- RequestTimingMiddleware.cs
+-- Models/
|   \-- WeatherForecast.cs
+-- Properties/
|   \-- launchSettings.json
+-- appsettings.Development.json
+-- appsettings.json
+-- CustomMiddleware.csproj
+-- CustomMiddleware.csproj.user
+-- CustomMiddleware.http
\-- ...
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/CustomMiddleware/CustomMiddleware.csproj
```

## Boas práticas e pontos de atenção

1. **Ordem importa**: Middlewares são executados na ordem de registro
2. **Performance**: Middlewares mais rápidos primeiro (exceto exception handling)
3. **Responsabilidade única**: Cada middleware deve ter uma função específica
4. **Configurabilidade**: Use options pattern para configurações
5. **Logging**: Sempre implemente logging adequado
6. **Async**: Use async/await corretamente

## Conteúdo complementar

##### Pipeline de Middleware

```
Request  →  [Middleware 1]  →  [Middleware 2]  →  [Middleware 3]  →  Endpoint
         ←                  ←                  ←                  ←
Response    [Middleware 1]  ←  [Middleware 2]  ←  [Middleware 3]  ←
```

##### Estrutura do Projeto

```
CustomMiddleware/
├── Middlewares/
│   ├── CorrelationIdMiddleware.cs      # Rastreamento de requisições
│   ├── CustomHeaderMiddleware.cs       # Headers personalizados
│   ├── RequestResponseLoggingMiddleware.cs  # Log de req/resp
│   ├── RequestTimingMiddleware.cs      # Medição de performance
│   └── Extensions/
│       ├── UseCustomMiddlewaresExtensions.cs    # Extension methods
│       └── UseDefaultMiddlewaresExtensions.cs
├── Models/                             # Models e configurações
├── Program.cs                          # Configuração do pipeline
└── README.md
```

##### 1. Middleware Básico - Request Timing

```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Processa próximo middleware
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            
            // Log da duração
            _logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                elapsedMs,
                context.Response.StatusCode);
                
            // Adiciona header com tempo de resposta
            context.Response.Headers.Append("X-Response-Time", $"{elapsedMs}ms");
        }
    }
}
```

##### 2. Middleware de Correlation ID

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Obtém ou cria correlation ID
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Armazena no HttpContext para uso posterior
        context.Items["CorrelationId"] = correlationId.ToString();
        
        // Adiciona ao response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append(CorrelationIdHeaderName, correlationId.ToString());
            return Task.CompletedTask;
        });

        // Adiciona ao escopo de log
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId.ToString()
        });

        await _next(context);
    }
}
```

##### 3. Middleware de Headers Customizados

```csharp
public class CustomHeaderOptions
{
    public Dictionary<string, string> Headers { get; set; } = new();
}

public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CustomHeaderOptions _options;

    public CustomHeaderMiddleware(RequestDelegate next, IOptions<CustomHeaderOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Adiciona headers customizados antes de processar
        context.Response.OnStarting(() =>
        {
            foreach (var header in _options.Headers)
            {
                if (!context.Response.Headers.ContainsKey(header.Key))
                {
                    context.Response.Headers.Append(header.Key, header.Value);
                }
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
```

##### 4. Middleware de Logging Avançado

```csharp
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log da requisição
        await LogRequest(context);

        // Captura response original
        var originalResponse = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            
            // Log da resposta
            await LogResponse(context);
        }
        finally
        {
            // Restaura response body
            await responseBody.CopyToAsync(originalResponse);
            context.Response.Body = originalResponse;
        }
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();
        var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
        context.Request.Body.Position = 0;

        _logger.LogInformation(
            "HTTP {Method} {Path} {QueryString} - Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            body);
    }

    private async Task LogResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        _logger.LogInformation(
            "HTTP Response {StatusCode} - Body: {Body}",
            context.Response.StatusCode,
            body);
    }
}
```

##### 1. Configuração no Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configurações dos middlewares
builder.Services.Configure<CustomHeaderOptions>(options =>
{
    options.Headers.Add("X-Powered-By", "My Custom App");
    options.Headers.Add("X-Content-Type-Options", "nosniff");
    options.Headers.Add("X-Frame-Options", "DENY");
});

var app = builder.Build();

// ORDEM IMPORTA! Middlewares são executados na ordem registrada
app.UseCustomMiddlewares();  // Extension method personalizado

// Middlewares padrão
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

##### 2. Extension Methods para Organização

```csharp
public static class UseCustomMiddlewaresExtensions
{
    public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder builder)
    {
        // Ordem específica para funcionalidade correta
        builder.UseMiddleware<RequestTimingMiddleware>();
        builder.UseMiddleware<CorrelationIdMiddleware>();
        builder.UseMiddleware<CustomHeaderMiddleware>();
        builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        
        return builder;
    }
}
```

##### 4. Testando os Middlewares

```bash
# Teste com correlation ID
curl -X GET https://localhost:7000/weatherforecast \
  -H "X-Correlation-ID: test-123" \
  -v

# Verificar headers de resposta
curl -I https://localhost:7000/weatherforecast

# Verificar timing
curl -X GET https://localhost:7000/weatherforecast -w "Total time: %{time_total}s\n"
```

##### 1. Middleware Condicional

```csharp
public class ConditionalMiddleware
{
    private readonly RequestDelegate _next;

    public ConditionalMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Só executa para endpoints específicos
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            // Lógica específica para APIs
            context.Response.Headers.Append("X-API-Version", "v1.0");
        }

        await _next(context);
    }
}
```

##### 2. Middleware com Configuração

```csharp
public class ConfigurableMiddlewareOptions
{
    public bool EnableLogging { get; set; } = true;
    public string[] ExcludePaths { get; set; } = Array.Empty<string>();
    public int MaxRequestSize { get; set; } = 1024 * 1024; // 1MB
}

public class ConfigurableMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConfigurableMiddlewareOptions _options;

    public ConfigurableMiddleware(RequestDelegate next, IOptions<ConfigurableMiddlewareOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Verifica se deve pular este path
        if (_options.ExcludePaths.Any(path => context.Request.Path.StartsWithSegments(path)))
        {
            await _next(context);
            return;
        }

        // Aplica lógica baseada na configuração
        if (_options.EnableLogging)
        {
            // Log da requisição
        }

        await _next(context);
    }
}
```

##### 3. Middleware de Error Handling

```csharp
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ArgumentNullException => new { status = 400, message = "Bad Request" },
            UnauthorizedAccessException => new { status = 401, message = "Unauthorized" },
            _ => new { status = 500, message = "Internal Server Error" }
        };

        context.Response.StatusCode = response.status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

##### Ordem Recomendada de Middlewares

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1. Error handling primeiro
app.UseMiddleware<RequestTimingMiddleware>();      // 2. Timing
app.UseMiddleware<CorrelationIdMiddleware>();      // 3. Correlation
app.UseMiddleware<RequestLoggingMiddleware>();     // 4. Logging
app.UseMiddleware<CustomHeaderMiddleware>();       // 5. Headers
app.UseAuthentication();                           // 6. Auth padrão
app.UseAuthorization();                           // 7. Authorization
app.UseRateLimiting();                            // 8. Rate limiting
// Endpoints por último
```

##### Evitar

1. **Middlewares pesados**: Evite operações demoradas
2. **Estado mutável**: Middlewares devem ser stateless
3. **Não chamar _next**: Sempre chame ou termine explicitamente
4. **Vazamentos**: Dispose recursos adequadamente
5. **Exception não tratadas**: Sempre trate exceções

##### Casos de Uso Comuns

| Middleware | Função | Quando Usar |
|-----------|--------|-------------|
| **Timing** | Mede performance | Monitoramento, debugging |
| **Correlation ID** | Rastreamento | Logs distribuídos, debugging |
| **Custom Headers** | Segurança, branding | Security headers, CORS |
| **Error Handling** | Tratamento global | APIs, error responses |
| **Rate Limiting** | Controle de tráfego | APIs públicas |
| **Compression** | Otimização | Responses grandes |
| **Authentication** | Segurança | Endpoints protegidos |

##### Exercícios Práticos

1. **Rate Limiting**: Implemente middleware de rate limiting
2. **Compression**: Adicione middleware de compressão
3. **API Versioning**: Crie middleware para versionamento
4. **Request Validation**: Valide requests antes de controllers
5. **Metrics**: Colete métricas de API (throughput, latência)

## Referências

- [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Custom Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [Middleware Order](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index#middleware-order)
- [Built-in Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index#built-in-middleware)

💡 **Dica**: Middlewares são a espinha dorsal de aplicações ASP.NET Core. Dominar sua criação e organização é essencial para construir APIs robustas e escaláveis!
