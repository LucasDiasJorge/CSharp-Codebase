# MyAsp — Client

## Descrição

Cliente TCP simples implementado com `TcpClient` (`System.Net.Sockets`). Conecta ao `Server` em `127.0.0.1:2126`, envia o token `"auth token"` e exibe a resposta recebida.

O projeto é intencionalmente básico para deixar claro o que qualquer cliente HTTP (como o `HttpClient` do .NET) resolve automaticamente.

## O que o código faz

```
Main()
 └── Client("127.0.0.1", 2126)
       └── Run()
             ├── TcpClient.Connect(ip, port)
             ├── NetworkStream.Write("auth token")
             └── NetworkStream.Read() → exibe resposta
```

## Como executar

> Certifique-se de que o `Server` já está rodando antes de executar o client.

```powershell
cd 03-WebAPIs/MyAsp/Client
dotnet run
```

Saída esperada:

```
Client starting...
Connecting to 127.0.0.1:2126...
Connected.
Sent: auth token
Received: Device authorization successfull
Connection closed.
```

## Decisões atuais e limitações

| Aspecto | Implementação atual | Limitação |
|---|---|---|
| Transporte | `TcpClient` síncrono | Bloqueia a thread durante connect, send e read |
| Protocolo | Bytes brutos em ASCII | Sem framing, sem headers, sem status codes |
| Autenticação | Token hardcoded na string `"auth token"` | Sem storage seguro, sem refresh, sem expiração |
| Resiliência | Nenhuma | Sem retry, sem timeout configurável, sem circuit-breaker |
| Testabilidade | `TcpClient` instanciado diretamente | Impossível mockar sem refatorar |
| Observabilidade | Sem logs ou métricas | Nenhuma visibilidade em produção |

## My ASP.NET from scratch — Client

Esta seção descreve o que precisaria evoluir neste `Client` para se tornar equivalente ao `HttpClient` do .NET — e o que vai além disso.

### 1. I/O assíncrono

Substituir chamadas síncronas por `async/await`:

```csharp
await tcpClient.ConnectAsync(ip, port, cancellationToken);
await stream.WriteAsync(sendBytes, cancellationToken);
int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
```

### 2. Protocolo HTTP em vez de bytes brutos

Em vez de enviar bytes ad hoc, usar HTTP/1.1:

```
POST /auth HTTP/1.1
Host: 127.0.0.1:2126
Content-Type: text/plain
Content-Length: 10

auth token
```

Ou simplesmente usar `HttpClient`:

```csharp
using HttpClient http = new();
HttpResponseMessage response = await http.PostAsync(
    "http://127.0.0.1:2126/auth",
    new StringContent("auth token"));
```

### 3. Gerenciamento de lifetime de conexões

`TcpClient` criado e descartado por chamada leva ao problema de esgotamento de portas. O equivalente correto é `IHttpClientFactory` com connection pooling e reuse:

```csharp
// Program.cs
services.AddHttpClient<AuthClient>(c => c.BaseAddress = new Uri("http://127.0.0.1:2126/"));
```

### 4. Resiliência com Polly

```csharp
services.AddHttpClient<AuthClient>()
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
```

### 5. Autenticação segura e gerenciamento de tokens

Centralizar aquisição e renovação de tokens em um `DelegatingHandler`:

```csharp
class AuthTokenHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", await _tokenProvider.GetTokenAsync(ct));
        return await base.SendAsync(request, ct);
    }
}
```

### 6. Serialização tipada

Substituir bytes ASCII brutos por contratos tipados com `System.Text.Json`:

```csharp
AuthResponse? result = await http.GetFromJsonAsync<AuthResponse>("/auth");
```

### 7. Testabilidade

Injetar `HttpMessageHandler` (ou usar `MockHttpMessageHandler`) para testes sem rede:

```csharp
HttpClient testClient = new(new FakeMessageHandler());
```

### 8. Observabilidade

- Propagar `traceparent` / `tracestate` headers (W3C Trace Context)
- Registrar métricas por endpoint (latência, erros, retentativas) via `System.Diagnostics.Metrics`
- Logs de request/response com `ILogger`

## Referências

- [HttpClient — Documentação oficial](https://learn.microsoft.com/dotnet/api/system.net.http.httpclient)
- [IHttpClientFactory](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory)
- [Polly — Resiliência](https://github.com/App-vNext/Polly)
- [System.Text.Json](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/overview)
- [W3C Trace Context](https://www.w3.org/TR/trace-context/)
