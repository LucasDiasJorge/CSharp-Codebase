# 👥 ShareableUser - Singleton Services e Thread Safety

## 🎯 Objetivos de Aprendizado

- Entender **padrão Singleton** em dependency injection
- Implementar **thread safety** em serviços compartilhados
- Detectar e tratar **condições de corrida** (race conditions)
- Usar **locks** para sincronização de threads
- Monitorar **concorrência** em aplicações web
- Aplicar **lifetimes de serviços** adequadamente

## 📚 Conceitos Fundamentais

### Singleton Pattern

O **padrão Singleton** garante que apenas uma instância de uma classe seja criada durante toda a execução da aplicação. No ASP.NET Core, serviços Singleton:

- ⚡ São criados **uma única vez**
- 🔄 São **reutilizados** em todas as requisições
- 💾 **Mantêm estado** entre requisições
- 🧵 Devem ser **thread-safe**

### Thread Safety

**Thread Safety** significa que o código pode ser executado simultaneamente por múltiplas threads sem causar inconsistências nos dados.

### Lifetimes de Serviços

| Lifetime | Escopo | Uso |
|----------|--------|-----|
| **Singleton** | Aplicação inteira | Cache, configurações, contadores |
| **Scoped** | Por requisição | DbContext, user context |
| **Transient** | Por injeção | Stateless services |

## 🏗️ Estrutura do Projeto

```
ShareableUser/
├── Services/
│   └── UserServices.cs         # Serviço singleton com thread safety
├── Middleware/
│   └── UserMiddleware.cs       # Middleware para monitoramento
├── Program.cs                  # Configuração e DI
├── appsettings.json           # Configurações
└── README.md
```

## 💡 Exemplos Práticos

### 1. Serviço Singleton Thread-Safe

```csharp
public interface IUsuarioSingletonService
{
    string NomeUsuario { get; set; }
    Guid InstanceId { get; }
    int ContadorAcessos { get; }
    void AcessarUsuario(string requestId);
    void AtualizarNome(string novoNome);
    Dictionary<string, int> ObterEstatisticas();
}

public class UsuarioSingletonService : IUsuarioSingletonService
{
    private readonly object _lock = new object();
    private readonly Dictionary<string, int> _acessosPorRequest = new();
    
    public string NomeUsuario { get; set; } = "Usuário Padrão";
    public Guid InstanceId { get; } = Guid.NewGuid();
    public int ContadorAcessos { get; private set; }
    
    private string _ultimoRequestId = string.Empty;

    public void AcessarUsuario(string requestId)
    {
        lock (_lock)
        {
            ContadorAcessos++;
            
            // Registra acesso por request
            if (_acessosPorRequest.ContainsKey(requestId))
                _acessosPorRequest[requestId]++;
            else
                _acessosPorRequest[requestId] = 1;

            // Detecta concorrência
            if (!string.IsNullOrEmpty(_ultimoRequestId) && _ultimoRequestId != requestId)
            {
                Console.WriteLine($"🔀 CONCORRÊNCIA DETECTADA! " +
                    $"Requests simultâneos: {_ultimoRequestId} e {requestId}");
            }
            
            _ultimoRequestId = requestId;
            
            // Simula processamento para evidenciar race conditions
            Thread.Sleep(1);
        }
    }

    public void AtualizarNome(string novoNome)
    {
        lock (_lock)
        {
            NomeUsuario = novoNome;
            Console.WriteLine($"✏️  Nome atualizado para: {novoNome}");
        }
    }

    public Dictionary<string, int> ObterEstatisticas()
    {
        lock (_lock)
        {
            // Retorna cópia para evitar modificações externas
            return new Dictionary<string, int>(_acessosPorRequest);
        }
    }
}
```

### 2. Versão com ConcurrentDictionary (Thread-Safe)

```csharp
public class UsuarioSingletonServiceConcurrent : IUsuarioSingletonService
{
    private readonly ConcurrentDictionary<string, int> _acessosPorRequest = new();
    private readonly object _lockNome = new object();
    
    private int _contadorAcessos;
    private string _nomeUsuario = "Usuário Padrão";
    
    public string NomeUsuario 
    { 
        get 
        {
            lock (_lockNome)
            {
                return _nomeUsuario;
            }
        }
        set 
        {
            lock (_lockNome)
            {
                _nomeUsuario = value;
            }
        }
    }
    
    public Guid InstanceId { get; } = Guid.NewGuid();
    
    public int ContadorAcessos => _contadorAcessos;

    public void AcessarUsuario(string requestId)
    {
        // Incremento atômico
        Interlocked.Increment(ref _contadorAcessos);
        
        // Thread-safe dictionary operation
        _acessosPorRequest.AddOrUpdate(requestId, 1, (key, value) => value + 1);
        
        Console.WriteLine($"🔗 Request {requestId} - Total acessos: {_contadorAcessos}");
    }

    public void AtualizarNome(string novoNome)
    {
        NomeUsuario = novoNome;
    }

    public Dictionary<string, int> ObterEstatisticas()
    {
        return new Dictionary<string, int>(_acessosPorRequest);
    }
}
```

### 3. Middleware de Monitoramento

```csharp
public class UsuarioMonitorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UsuarioMonitorMiddleware> _logger;

    public UsuarioMonitorMiddleware(
        RequestDelegate next,
        ILogger<UsuarioMonitorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUsuarioSingletonService usuarioService)
    {
        // Gera ID único para esta requisição
        var requestId = GenerateRequestId();
        context.Items["RequestId"] = requestId;
        
        // Timestamp de início
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation(
            "🚀 Iniciando Request {RequestId} - Usuario Instance: {InstanceId}", 
            requestId, usuarioService.InstanceId);

        // Registra acesso ao serviço singleton
        usuarioService.AcessarUsuario(requestId);
        
        // Simula delay para evidenciar concorrência
        var delay = Random.Shared.Next(50, 200);
        await Task.Delay(delay);
        
        try
        {
            // Continua pipeline
            await _next(context);
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation(
                "✅ Finalizando Request {RequestId} - Duração: {Duration}ms - Total Acessos: {Acessos}",
                requestId, duration.TotalMilliseconds, usuarioService.ContadorAcessos);
        }
    }

    private static string GenerateRequestId()
    {
        return Guid.NewGuid().ToString()[..8];
    }
}
```

### 4. Endpoints para Demonstração

```csharp
// Program.cs - Configuração de endpoints
app.MapGet("/", (IUsuarioSingletonService usuario, HttpContext context) => 
{
    var requestId = context.Items["RequestId"]?.ToString() ?? "unknown";
    
    return Results.Ok(new {
        Usuario = usuario.NomeUsuario,
        InstanceId = usuario.InstanceId,
        Acessos = usuario.ContadorAcessos,
        RequestId = requestId
    });
});

app.MapPost("/usuario/nome", (UpdateNameRequest request, IUsuarioSingletonService usuario) =>
{
    usuario.AtualizarNome(request.NovoNome);
    
    return Results.Ok(new {
        Message = "Nome atualizado com sucesso",
        NovoNome = usuario.NomeUsuario
    });
});

app.MapGet("/estatisticas", (IUsuarioSingletonService usuario) =>
{
    var stats = usuario.ObterEstatisticas();
    
    return Results.Ok(new {
        TotalAcessos = usuario.ContadorAcessos,
        InstanceId = usuario.InstanceId,
        AcessosPorRequest = stats,
        RequestsUnicos = stats.Count
    });
});

app.MapGet("/teste-concorrencia", async (IUsuarioSingletonService usuario) =>
{
    // Simula múltiplas operações simultâneas
    var tasks = Enumerable.Range(1, 10)
        .Select(i => Task.Run(() => 
        {
            var requestId = $"test-{i}";
            usuario.AcessarUsuario(requestId);
        }))
        .ToArray();
    
    await Task.WhenAll(tasks);
    
    return Results.Ok(new {
        Message = "Teste de concorrência executado",
        TotalAcessos = usuario.ContadorAcessos
    });
});

public record UpdateNameRequest(string NovoNome);
```

### 5. Demonstração de Race Conditions

```csharp
public class RaceConditionDemo
{
    private readonly IUsuarioSingletonService _service;

    public RaceConditionDemo(IUsuarioSingletonService service)
    {
        _service = service;
    }

    public async Task DemonstrarRaceCondition()
    {
        Console.WriteLine("🏁 Iniciando demonstração de Race Condition...\n");

        // Cria múltiplas tasks que acessam o serviço simultaneamente
        var tasks = new List<Task>();
        
        for (int i = 1; i <= 20; i++)
        {
            int taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                var requestId = $"task-{taskId:D2}";
                
                // Acessa o serviço
                _service.AcessarUsuario(requestId);
                
                // Simula processamento
                await Task.Delay(Random.Shared.Next(10, 100));
                
                Console.WriteLine($"Task {taskId:D2} completada - Acessos: {_service.ContadorAcessos}");
            }));
        }

        // Aguarda todas as tasks
        await Task.WhenAll(tasks);
        
        Console.WriteLine($"\n✅ Total de acessos: {_service.ContadorAcessos}");
        Console.WriteLine($"📊 Estatísticas: {string.Join(", ", _service.ObterEstatisticas().Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
    }
}
```

## 🚀 Configuração e Execução

### 1. Registro do Serviço Singleton

```csharp
var builder = WebApplication.CreateBuilder(args);

// ⚠️ ATENÇÃO: Singleton mantém estado entre requisições
builder.Services.AddSingleton<IUsuarioSingletonService, UsuarioSingletonService>();

// Para comparação - outros lifetimes
builder.Services.AddScoped<IScopedService, ScopedService>();
builder.Services.AddTransient<ITransientService, TransientService>();

// Logging para observar comportamento
builder.Services.AddLogging(configure => 
{
    configure.AddConsole()
             .SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Middleware de monitoramento
app.UseMiddleware<UsuarioMonitorMiddleware>();

app.Run();
```

### 2. Executar o Projeto

```bash
# Navegar para o diretório
cd ShareableUser

# Restaurar dependências
dotnet restore

# Executar a aplicação
dotnet run
```

### 3. Testes de Concorrência

```bash
# Teste básico
curl http://localhost:5000/

# Atualizar nome
curl -X POST http://localhost:5000/usuario/nome \
  -H "Content-Type: application/json" \
  -d '{"novoNome": "João Silva"}'

# Ver estatísticas
curl http://localhost:5000/estatisticas

# Teste de concorrência automático
curl http://localhost:5000/teste-concorrencia

# Múltiplas requisições simultâneas (Linux/Mac)
for i in {1..10}; do curl http://localhost:5000/ & done; wait

# Múltiplas requisições simultâneas (Windows PowerShell)
1..10 | ForEach-Object { Start-Job -ScriptBlock { Invoke-RestMethod http://localhost:5000/ } }
```

## 🔧 Cenários de Thread Safety

### 1. Sem Thread Safety (❌ PERIGOSO)

```csharp
public class UnsafeCounter
{
    private int _count = 0;
    
    public int Count => _count;
    
    public void Increment()
    {
        // ❌ Race condition aqui!
        _count++; // Não é operação atômica
    }
}
```

### 2. Com Lock (✅ SEGURO mas pode ter bottleneck)

```csharp
public class SafeCounterWithLock
{
    private readonly object _lock = new object();
    private int _count = 0;
    
    public int Count 
    { 
        get 
        { 
            lock (_lock) 
            { 
                return _count; 
            } 
        } 
    }
    
    public void Increment()
    {
        lock (_lock)
        {
            _count++; // ✅ Thread-safe
        }
    }
}
```

### 3. Com Interlocked (✅ SEGURO e performático)

```csharp
public class SafeCounterWithInterlocked
{
    private int _count = 0;
    
    public int Count => _count;
    
    public void Increment()
    {
        Interlocked.Increment(ref _count); // ✅ Atômico e rápido
    }
}
```

## 💯 Melhores Práticas

### ✅ Thread Safety

1. **Use locks** para operações complexas
2. **Prefira Interlocked** para operações simples
3. **Use ConcurrentCollections** quando apropriado
4. **Minimize tempo em locks** críticos
5. **Evite locks aninhados** (deadlock risk)

### ✅ Singleton Services

1. **Torne thread-safe** por padrão
2. **Evite estado mutável** quando possível
3. **Use para cache** e configurações
4. **Documente comportamento** de concorrência
5. **Teste sob carga** para detectar race conditions

### ❌ Evitar

1. **Estado mutável sem sincronização**
2. **Locks muito granulares** (performance)
3. **Locks muito amplos** (bottleneck)
4. **Operações I/O dentro de locks**
5. **Singleton para dados por usuário**

## 🔍 Detectando Race Conditions

### Ferramentas e Técnicas

```csharp
public class RaceConditionDetector
{
    private readonly ConcurrentDictionary<string, DateTime> _activeOperations = new();
    
    public void DetectConcurrency(string operationId)
    {
        var startTime = DateTime.UtcNow;
        
        if (!_activeOperations.TryAdd(operationId, startTime))
        {
            Console.WriteLine($"⚠️  Operação {operationId} já está em andamento!");
        }
        
        try
        {
            // Simula operação
            Thread.Sleep(Random.Shared.Next(10, 50));
        }
        finally
        {
            if (_activeOperations.TryRemove(operationId, out var originalTime))
            {
                var duration = DateTime.UtcNow - originalTime;
                if (duration.TotalMilliseconds > 100)
                {
                    Console.WriteLine($"⏱️  Operação {operationId} demorou {duration.TotalMilliseconds:F1}ms");
                }
            }
        }
    }
}
```

## 📋 Exercícios Práticos

1. **Implementar Cache**: Crie cache thread-safe com expiração
2. **Contador Global**: Implemente contador compartilhado entre requisições
3. **Session Manager**: Gerencie sessões de usuário com Singleton
4. **Rate Limiter**: Crie rate limiting usando Singleton
5. **Metrics Collector**: Colete métricas de performance da aplicação

## 🔗 Recursos Adicionais

- [Thread Safety in .NET](https://docs.microsoft.com/en-us/dotnet/standard/threading/thread-safety)
- [Dependency Injection Lifetimes](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#service-lifetimes)
- [Concurrent Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [Interlocked Class](https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked)

---

💡 **Dica**: Singleton services são úteis para dados compartilhados, mas sempre implemente thread safety adequada. Race conditions são bugs sutis que aparecem apenas sob carga, então teste sempre com múltiplas requisições simultâneas!
