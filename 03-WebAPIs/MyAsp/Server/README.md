# MyAsp — Server

## Descrição

Servidor TCP multi-threaded implementado sobre `System.Net.Sockets`, sem qualquer framework de alto nível. Cada cliente é atendido em uma `Thread` dedicada; o servidor valida um token de autorização simples e responde com uma mensagem de confirmação.

O projeto é intencionalmente minimalista para expor os fundamentos do que um servidor HTTP (como o Kestrel do ASP.NET Core) faz internamente.

## O que o código faz

```
Main()
 └── Thread → Server("127.0.0.1", 2126)
                └── TcpListener.Start()
                └── StartListener()  ← loop infinito
                      └── AcceptTcpClient() → Thread → HandleConnection(client)
                                                          ├── Lê bytes do NetworkStream
                                                          ├── Valida "auth token"
                                                          └── Envia "Device authorization successfull"
```

## Como executar

```powershell
cd 03-WebAPIs/MyAsp/Server
dotnet run
```

Saída esperada:

```
Server started...
Waiting for Incoming connections ...
Connected to authorized client: 1
<ThreadId>: Received: auth token
<ThreadId>: Sent: Device authorization successfull
```

## Decisões atuais e limitações

| Aspecto | Implementação atual | Limitação |
|---|---|---|
| Concorrência | Thread por conexão | Não escala para muitas conexões simultâneas |
| I/O | `NetworkStream.Read` síncrono | Bloqueia a thread enquanto espera dados |
| Buffer | `byte[256]` fixo | Mensagens maiores que 256 bytes são truncadas |
| Autenticação | Comparação de string direta | Sem hashing, sem expiração, sem refresh |
| Ciclo de vida | Loop `while(true)` sem shutdown | Não tem graceful shutdown |
| Erros | `try/catch` local por conexão | Sem estratégia global de tratamento de erros |

## My ASP.NET from scratch — Server

Esta seção descreve o que precisaria evoluir neste `Server` para se tornar a base de um servidor HTTP real.

### 1. Listener assíncrono (I/O não bloqueante)

Substituir threads bloqueantes por `async/await`:

```csharp
while (!cancellationToken.IsCancellationRequested)
{
    TcpClient client = await server.AcceptTcpClientAsync(cancellationToken);
    _ = Task.Run(() => HandleConnectionAsync(client, cancellationToken));
}
```

Para alta performance: `System.IO.Pipelines` (`PipeReader`/`PipeWriter`) com zero-copy.

### 2. Framing de protocolo

Bytes brutos sem delimitador são ambíguos. Adicionar length-prefix:

```
[ 4 bytes: tamanho do payload ][ N bytes: payload ]
```

Ou usar HTTP/1.1: `GET / HTTP/1.1\r\nHost: ...\r\n\r\n`.

### 3. Pipeline de middlewares

Encadear responsabilidades em vez de colocar tudo em `HandleConnection`:

```
Conexão → [Logging] → [Autenticação] → [Roteamento] → [Handler] → Resposta
```

Cada middleware recebe o contexto e chama o próximo (`RequestDelegate next`), exatamente como o ASP.NET Core faz.

### 4. Abstração de Request/Response

Criar um `HttpContext` próprio:

```csharp
class MyHttpContext
{
    public MyRequest Request { get; }
    public MyResponse Response { get; }
    public IServiceProvider Services { get; }
}
```

### 5. Injeção de Dependência

Registrar serviços em um `IServiceProvider` e resolver por escopo de requisição:

```csharp
services.AddScoped<IAuthenticator, TokenAuthenticator>();
services.AddSingleton<IRouter, Router>();
```

### 6. Graceful Shutdown

Escutar `SIGTERM` / `Ctrl+C` via `CancellationToken` e aguardar conexões ativas encerrarem:

```csharp
using CancellationTokenSource cts = new();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
await server.RunAsync(cts.Token);
```

### 7. Observabilidade

- Logs estruturados com `ILogger<T>` (Serilog, etc.)
- Métricas com `System.Diagnostics.Metrics` (connections, requests/s, latency)
- Tracing com `Activity`/`ActivitySource` e propagação de `traceparent`

### 8. Segurança

- Substituir token plaintext por JWT com validação de assinatura e expiração
- Adicionar `SslStream` para TLS
- Rate limiting por IP (ex.: `System.Threading.RateLimiting`)
- Validar e sanitizar todo input antes de processar

## Referências

- [System.Net.Sockets — TcpListener](https://learn.microsoft.com/dotnet/api/system.net.sockets.tcplistener)
- [System.IO.Pipelines](https://learn.microsoft.com/dotnet/standard/io/pipelines)
- [ASP.NET Core Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware)
- [ASP.NET Core — Kestrel web server](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel)
- [CancellationToken e Graceful Shutdown](https://learn.microsoft.com/dotnet/core/extensions/cancellation-tokens)
