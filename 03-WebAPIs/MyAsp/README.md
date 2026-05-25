# MyAsp

## Descrição

Par `Server` + `Client` implementados diretamente sobre sockets TCP (`System.Net.Sockets`), sem nenhum framework de alto nível. O objetivo é expor o que acontece nas camadas mais baixas de rede e usar esse conhecimento como base para evoluir ao modelo de um servidor HTTP/ASP.NET feito do zero.

- O `Server` escuta conexões na porta `2126`, valida um token simples (`"auth token"`) e responde com uma mensagem de autorização.
- O `Client` conecta em `127.0.0.1:2126`, envia o token e exibe a resposta.

## Estrutura

```
MyAsp/
├── Server/
│   └── Program.cs   ← TcpListener + Thread por conexão + validação de token
└── Client/
    └── Program.cs   ← TcpClient + envio de token + leitura de resposta
```

## Como executar

> Execute o `Server` antes do `Client`.

**Server**

```powershell
cd 03-WebAPIs/MyAsp/Server
dotnet run
```

**Client** (em outro terminal)

```powershell
cd 03-WebAPIs/MyAsp/Client
dotnet run
```

Saída esperada no `Client`:

```
Client starting...
Connecting to 127.0.0.1:2126...
Connected.
Sent: auth token
Received: Device authorization successfull
Connection closed.
```

## My ASP.NET from scratch

Esta seção mapeia cada decisão atual do projeto para o equivalente em um servidor HTTP real, mostrando o que precisaria evoluir para se tornar um "ASP.NET from scratch".

### Ciclo de vida e Host

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| `Main()` instancia `Server` diretamente | Classe `Host` com `Build()` + `Run()` e ciclo de vida (`IHostedService`) |
| Sem inicialização ordenada | `StartAsync` / `StopAsync` com `CancellationToken` e graceful shutdown |
| Sem configuração externa | `IConfiguration` lendo `appsettings.json` e variáveis de ambiente |

### Pipeline de requisição (Middlewares)

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| `HandleConnection` trata tudo num único método | Pipeline de middlewares encadeados (`RequestDelegate next`) |
| Validação de token hardcoded no handler | Middleware de autenticação dedicado |
| Sem logging estruturado | Middleware de logging com `ILogger<T>` e request/response logging |
| Exceções capturadas localmente | Middleware global de tratamento de exceções |

### Roteamento e Handlers

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| Apenas uma ação (enviar mensagem de auth) | Roteador que mapeia método + path → handler |
| Sem abstração de request/response | `HttpContext` com `Request` e `Response` tipados |
| Protocolo ad hoc (bytes brutos) | HTTP/1.1 ou HTTP/2: parsing de headers, status codes, body |

### Injeção de Dependência

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| `Worker` instanciado diretamente dentro de `Server` | `IServiceProvider` com escopos por requisição |
| Sem interfaces | Registrar `IAuthenticator`, `IRequestParser`, etc. |

### I/O e Performance

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| Thread por conexão (bloqueante) | `async/await` com `AcceptTcpClientAsync`, `ReadAsync`, `WriteAsync` |
| `NetworkStream.Read` síncrono | `System.IO.Pipelines` (`PipeReader`/`PipeWriter`) para zero-copy |
| Buffer fixo de 256 bytes | Framing com length-prefix ou delimitador para mensagens variáveis |

### Observabilidade

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| `Console.WriteLine` | Logs estruturados via `ILogger` (Serilog, etc.) |
| Sem métricas | `System.Diagnostics.Metrics` + OpenTelemetry |
| Sem tracing | `Activity` / `ActivitySource` com trace/span IDs propagados |

### Segurança

| Hoje (MyAsp) | ASP.NET from scratch |
|---|---|
| Token plaintext sem validação segura | JWT / HMAC com validação de assinatura e expiração |
| Sem TLS | `SslStream` sobre `TcpClient` / `TcpListener` |
| Sem rate limiting | Middleware de rate limiting por IP/token |
| Input não sanitizado | Validação e sanitização antes de processar qualquer payload |

## Referências

- [System.Net.Sockets](https://learn.microsoft.com/dotnet/api/system.net.sockets)
- [System.IO.Pipelines](https://learn.microsoft.com/dotnet/standard/io/pipelines)
- [ASP.NET Core Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware)
- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
