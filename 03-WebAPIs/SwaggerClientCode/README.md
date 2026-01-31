# 🔧 SwaggerClientCode - Geração Automática de Clientes de API

## 🎯 Objetivos de Aprendizado

- Integrar **Swagger/OpenAPI** em APIs ASP.NET Core
- Gerar **clientes de API automaticamente** a partir de especificações OpenAPI
- Usar **NSwag** para geração de código
- Implementar **testes de integração** com clientes gerados
- Configurar **documentação interativa** com SwaggerUI
- Aplicar **versionamento de API** e **contratos tipados**

## 📚 Conceitos Fundamentais

### OpenAPI/Swagger

**OpenAPI** (anteriormente Swagger) é uma especificação para descrever APIs REST. Permite:

- 📝 **Documentação**: Especifica endpoints, parâmetros, responses
- 🔧 **Geração de código**: Cria clientes automaticamente
- 🧪 **Testes**: Interface interativa para testar APIs
- 📋 **Contratos**: Define interface clara entre frontend/backend

### NSwag vs Swashbuckle

| Aspecto | NSwag | Swashbuckle |
|---------|-------|-------------|
| **Documentação** | ✅ Sim | ✅ Sim |
| **Geração de Cliente** | ✅ C#, TS, etc. | ❌ Não |
| **Ferramentas CLI** | ✅ Robusto | ⚠️ Limitado |
| **Configuração** | 🔧 Complexa | 🎯 Simples |
| **Performance** | ⚡ Otimizada | ⚡ Boa |

## 🏗️ Estrutura do Projeto

```
SwaggerClientCode/
├── Controllers/
│   └── UserController.cs          # API endpoints
├── ClientGenerator.cs             # Gerador de clientes
├── GeneratedApiClient.cs          # Cliente gerado automaticamente
├── Program.cs                     # Configuração e demonstração
├── appsettings.json              # Configurações
└── README.md
```

## 💡 Exemplos Práticos

### 1. Configuração do Swagger

```csharp
// Program.cs - Configuração da API
var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuração do Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "My API", 
        Version = "v1",
        Description = "Uma API de exemplo para demonstrar Swagger",
        Contact = new OpenApiContact
        {
            Name = "Desenvolvedor",
            Email = "dev@example.com"
        }
    });
    
    // Inclui comentários XML
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    
    // Configuração de autenticação
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
});

var app = builder.Build();

// Middleware do Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
        c.DocumentTitle = "My API Documentation";
        c.DisplayRequestDuration();
    });
}

app.MapControllers();
app.Run();
```

### 2. Controller com Documentação

```csharp
/// <summary>
/// Controller para gerenciar usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    /// <summary>
    /// Obtém um usuário pelo ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Dados do usuário</returns>
    /// <response code="200">Usuário encontrado</response>
    /// <response code="404">Usuário não encontrado</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        if (id <= 0)
            return BadRequest("ID deve ser maior que zero");
            
        var user = await _userService.GetByIdAsync(id);
        
        if (user == null)
            return NotFound($"Usuário com ID {id} não encontrado");
            
        return Ok(user);
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    /// <param name="user">Dados do usuário</param>
    /// <returns>Usuário criado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUserRequest user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var createdUser = await _userService.CreateAsync(user);
        
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
    }
}
```

### 3. Modelos com Validação

```csharp
/// <summary>
/// Representa um usuário do sistema
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único do usuário
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }
    
    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    /// <example>João Silva</example>
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email do usuário
    /// </summary>
    /// <example>joao@example.com</example>
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dados para criação de usuário
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Nome do usuário
    /// </summary>
    /// <example>Maria Santos</example>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Email do usuário
    /// </summary>
    /// <example>maria@example.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
```

### 4. Geração Automática de Cliente

```csharp
public class ClientGenerator
{
    public async Task GenerateClientAsync(string swaggerUrl, string outputPath)
    {
        try
        {
            // Baixa especificação OpenAPI
            using var httpClient = new HttpClient();
            var swaggerJson = await httpClient.GetStringAsync(swaggerUrl);
            var document = await OpenApiDocument.FromJsonAsync(swaggerJson);

            // Configurações do gerador
            var settings = new CSharpClientGeneratorSettings
            {
                ClassName = "ApiClient",
                CSharpGeneratorSettings = 
                {
                    Namespace = "MyApi.Client",
                    JsonLibrary = CSharpJsonLibrary.SystemTextJson,
                    GenerateJsonMethods = true,
                    GenerateOptionalParameters = true
                },
                GenerateExceptionClasses = true,
                ExceptionClass = "ApiException",
                WrapDtoExceptions = true,
                UseHttpClientCreationMethod = false,
                GenerateClientInterfaces = true,
                ClientInterfaceName = "IApiClient",
                GenerateResponseClasses = true,
                WrapResponses = true,
                ResponseClass = "ApiResponse"
            };

            // Gera código do cliente
            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Salva arquivo
            await File.WriteAllTextAsync(outputPath, code);
            Console.WriteLine($"Cliente gerado em: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao gerar cliente: {ex.Message}");
            throw;
        }
    }
}
```

### 5. Uso do Cliente Gerado

```csharp
public class ApiClientService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<ApiClientService> _logger;

    public ApiClientService(IApiClient apiClient, ILogger<ApiClientService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task DemonstrateApiUsage()
    {
        try
        {
            // Listar usuários
            var users = await _apiClient.UsersAllAsync();
            _logger.LogInformation("Encontrados {Count} usuários", users.Count);

            // Criar usuário
            var newUser = new CreateUserRequest
            {
                Name = "Test User",
                Email = "test@example.com"
            };
            
            var createdUser = await _apiClient.UsersPostAsync(newUser);
            _logger.LogInformation("Usuário criado com ID: {Id}", createdUser.Id);

            // Buscar usuário específico
            var user = await _apiClient.UsersGetAsync(createdUser.Id);
            _logger.LogInformation("Usuário encontrado: {Name}", user.Name);

            // Atualizar usuário
            user.Name = "Updated Name";
            await _apiClient.UsersPutAsync(user.Id, user);
            
            // Deletar usuário
            await _apiClient.UsersDeleteAsync(user.Id);
            _logger.LogInformation("Usuário deletado");
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Erro na API: {StatusCode} - {Response}", 
                ex.StatusCode, ex.Response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado");
        }
    }
}
```

### 6. Configuração de DI para Cliente

```csharp
// Program.cs - Registro do cliente
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Ou com Polly para retry
builder.Services.AddHttpClient<IApiClient, ApiClient>()
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, delay, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {delay}s");
            });
}
```

## 🚀 Configuração e Execução

### 1. Dependências necessárias

```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="NSwag.CodeGeneration.CSharp" Version="13.20.0" />
<PackageReference Include="NSwag.Core" Version="13.20.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
```

### 2. Habilitar Comentários XML

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\Debug\net8.0\SwaggerClientCode.xml</DocumentationFile>
</PropertyGroup>
```

### 3. Executar o Projeto

```bash
# Navegar para o diretório
cd SwaggerClientCode

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run

# Acessar documentação
# http://localhost:5000/swagger
```

### 4. Gerar Cliente via CLI

```bash
# Instalar ferramenta NSwag
dotnet tool install -g NSwag.ConsoleCore

# Gerar cliente
nswag openapi2csclient /input:http://localhost:5000/swagger/v1/swagger.json /output:GeneratedClient.cs /namespace:MyApi.Client
```

## 🔧 Configurações Avançadas

### 1. Versionamento de API

```csharp
// Configuração de versionamento
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new QueryStringApiVersionReader("version")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Swagger para múltiplas versões
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API", Version = "v2" });
});
```

### 2. Autenticação JWT no Swagger

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### 3. Filtros Customizados

```csharp
public class SwaggerExcludeFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null)
            return;

        var excludedProperties = context.Type.GetProperties()
            .Where(t => t.GetCustomAttribute<SwaggerIgnoreAttribute>() != null);

        foreach (var excludedProperty in excludedProperties)
        {
            var propertyToRemove = schema.Properties.Keys
                .SingleOrDefault(x => x.ToLower() == excludedProperty.Name.ToLower());
                
            if (propertyToRemove != null)
                schema.Properties.Remove(propertyToRemove);
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class SwaggerIgnoreAttribute : Attribute
{
}
```

## 💯 Melhores Práticas

### ✅ Documentação

1. **Use comentários XML** em controllers e modelos
2. **Defina examples** para requests/responses
3. **Documente status codes** possíveis
4. **Inclua validações** nos modelos
5. **Organize por versões** quando necessário

### ✅ Geração de Cliente

1. **Configure namespace** apropriado
2. **Gere interfaces** para facilitar testes
3. **Use exception classes** customizadas
4. **Configure timeout** adequado
5. **Implemente retry policies**

### ❌ Evitar

1. **Endpoints sem documentação**
2. **Modelos sem validação**
3. **Status codes genéricos**
4. **Clientes sem tratamento de erro**
5. **Hardcoded URLs** no cliente

## 🔍 Debugging e Troubleshooting

### Problemas Comuns

| Problema | Causa | Solução |
|----------|-------|---------|
| **Cliente não gerado** | URL inválida | Verificar se API está rodando |
| **Propriedades missing** | Naming policy | Configurar case insensitive |
| **Timeouts** | Operações longas | Aumentar timeout do HttpClient |
| **Auth failures** | Token inválido | Verificar configuração de auth |

### Debug do Cliente Gerado

```csharp
public class DebuggingApiClient : IApiClient
{
    private readonly IApiClient _inner;
    private readonly ILogger<DebuggingApiClient> _logger;

    public DebuggingApiClient(IApiClient inner, ILogger<DebuggingApiClient> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<User> UsersGetAsync(int id)
    {
        _logger.LogInformation("Calling UsersGetAsync with ID: {Id}", id);
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _inner.UsersGetAsync(id);
            _logger.LogInformation("UsersGetAsync completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UsersGetAsync failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

## 📋 Exercícios Práticos

1. **API Completa**: Crie CRUD completo com documentação
2. **Multi-versioning**: Implemente versionamento v1 e v2
3. **Authentication**: Adicione JWT ao Swagger e cliente
4. **Error Handling**: Implemente tratamento de erros robusto
5. **Performance**: Benchmark clientes gerados vs manuais

## 🔗 Recursos Adicionais

- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [NSwag Documentation](https://github.com/RicoSuter/NSwag)
- [OpenAPI Specification](https://swagger.io/specification/)
- [ASP.NET Core API Documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger)

---

💡 **Dica**: A combinação de Swagger para documentação e NSwag para geração de clientes cria um ecossistema robusto onde mudanças na API são automaticamente refletidas nos clientes, reduzindo erros e acelerando o desenvolvimento!