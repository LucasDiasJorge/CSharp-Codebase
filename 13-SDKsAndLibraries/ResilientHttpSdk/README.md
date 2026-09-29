# ResilientHttpSdk

SDK HTTP tipado com `IHttpClientFactory`, configuração por Options Pattern, cancelamento, retry seletivo e tradução consistente de erros — com suíte xUnit que verifica cada uma dessas decisões.

## Visão geral

Um SDK que devolve `HttpResponseMessage` e deixa escapar `HttpRequestException` obriga cada chamador a reaprender HTTP. Este exemplo mostra as decisões que transformam um invólucro de `HttpClient` em uma biblioteca com API própria.

**Tradução de erro** é a principal. Cada status vira um tipo: 404 → `SdkNotFoundException`, 400/422 → `SdkValidationException` com os erros por campo já extraídos do `ProblemDetails`, 401/403 → `SdkAuthenticationException`, 429 → `SdkRateLimitException` carregando o `Retry-After`, 5xx → `SdkServerException`, e falha de rede → `SdkTransportException`. O chamador escreve `catch (SdkNotFoundException)`, não `if (response.StatusCode == 404)`.

**Retry seletivo**: 5xx, 408 e 429 merecem nova tentativa; 400, 404 e 401 não — repetir não muda o resultado e só multiplica carga. Medido na suíte: 503 seguido de 200 faz **2 tentativas**; 503 persistente com `MaxRetryAttempts = 3` faz **4** (a original mais três); 400 faz **1**.

**Cancelamento** é o ponto mais fácil de errar. Timeout do `HttpClient` e cancelamento do chamador chegam como a **mesma** `OperationCanceledException`. O SDK distingue pelo `token.IsCancellationRequested`: se o chamador cancelou, a exceção é propagada intacta; se foi o timeout interno, vira `SdkTransportException`. Traduzir os dois casos igual quebra o `catch (OperationCanceledException)` de quem chama.

**Configuração** por `ResilientHttpSdkOptions` com `ValidateDataAnnotations().ValidateOnStart()`: `BaseAddress` ausente derruba a aplicação no start, não na primeira chamada.

## Conceitos abordados

- Cliente tipado registrado por `AddHttpClient<TInterface, TImplementation>`.
- `IHttpClientFactory` e o ciclo de vida do handler.
- Options Pattern com validação no start.
- Hierarquia de exceções própria e tradução de status HTTP.
- `ProblemDetails` lido e transformado em erros por campo.
- `Retry-After` exposto ao chamador.
- Retry com backoff exponencial e jitter via `Microsoft.Extensions.Http.Resilience`.
- Distinção entre cancelamento do chamador e timeout interno.
- `HttpMessageHandler` de teste para exercitar o pipeline sem rede.

## Objetivos de aprendizagem

- Projetar a superfície pública de um SDK sem vazar detalhes de transporte.
- Decidir o que merece nova tentativa e o que não merece.
- Propagar cancelamento corretamente em biblioteca assíncrona.
- Validar configuração de SDK no start da aplicação hospedeira.
- Testar SDK HTTP sem rede, exercitando o pipeline real de resiliência.

## Estrutura do projeto

```text
ResilientHttpSdk/
`-- src/
    |-- ResilientHttpSdk/
    |   |-- ProductsClient.cs
    |   |-- Products.cs
    |   |-- ResilientHttpSdkOptions.cs
    |   |-- SdkExceptions.cs
    |   |-- ServiceCollectionExtensions.cs
    |   `-- ResilientHttpSdk.csproj
    `-- ResilientHttpSdk.Tests/
        |-- SdkBehaviorTests.cs
        |-- SdkTestHost.cs
        |-- StubHttpMessageHandler.cs
        `-- ResilientHttpSdk.Tests.csproj
```

## Como executar

```bash
dotnet test 13-SDKsAndLibraries/ResilientHttpSdk/src/ResilientHttpSdk.Tests/ResilientHttpSdk.Tests.csproj
```

Com a saída das contagens de tentativa:

```bash
dotnet test 13-SDKsAndLibraries/ResilientHttpSdk/src/ResilientHttpSdk.Tests/ResilientHttpSdk.Tests.csproj --logger "console;verbosity=detailed"
```

Para validar apenas a compilação da biblioteca:

```bash
dotnet build 13-SDKsAndLibraries/ResilientHttpSdk/src/ResilientHttpSdk/ResilientHttpSdk.csproj
```

São 23 testes, todos passando. Não exige serviço externo — o handler de teste substitui a rede.

Como um consumidor usaria:

```csharp
builder.Services.AddResilientHttpSdk(builder.Configuration);   // secao "ResilientHttpSdk"

// ou, sem arquivo de configuracao:
builder.Services.AddResilientHttpSdk(options =>
{
    options.BaseAddress = new Uri("https://api.exemplo.com");
    options.ApiKey = apiKey;
    options.MaxRetryAttempts = 3;
});
```

## Boas práticas e pontos de atenção

- Ofereça **um** `Add*` e nada mais. Quem consome não deveria precisar saber que existe `HttpClient`, handler ou política de retry por dentro.
- Nunca dê `new HttpClient()` dentro do SDK. Isso esgota sockets sob carga e ignora mudança de DNS; `IHttpClientFactory` existe para resolver as duas coisas.
- Receba `CancellationToken` em **todo** método público, com valor padrão. SDK que não aceita token impede o chamador de desistir, e nenhum invólucro conserta isso depois.
- Propague `OperationCanceledException` quando o token do chamador foi cancelado. Traduzir para erro próprio nesse caso é bug — e como as duas situações chegam pela mesma exceção, o `when (token.IsCancellationRequested)` é obrigatório.
- Traduza status em tipos, mas **exponha** o status na exceção. Esconder totalmente prejudica diagnóstico; o que não se quer é obrigar o chamador a olhar.
- Não repita erro do cliente. 400, 404, 401 e 422 dão o mesmo resultado na segunda tentativa.
- Cuidado com retry em `POST`. A suíte mostra dois `POST` enviados para uma chamada: sem chave de idempotência, o servidor pode criar dois recursos. Para operação não idempotente, `MaxRetryAttempts = 0` ou uma `Idempotency-Key`.
- Use jitter no backoff. Sem ele, todos os clientes que falharam juntos voltam juntos, e o retry vira ataque ao próprio serviço.
- Trate corpo de erro que não é JSON. Proxy e gateway devolvem HTML; um SDK que só sabe ler `ProblemDetails` quebra exatamente quando a infraestrutura falha.
- `BaseAddress` precisa terminar em `/`. Sem a barra, o último segmento do caminho é **substituído** em vez de concatenado, e `https://api.exemplo.com/v2` + `products/1` viraria `https://api.exemplo.com/products/1`. O SDK acrescenta a barra por isso, e há um teste fixando esse comportamento.
- Valide options no start com `ValidateOnStart()`. Configuração errada tem de derrubar o deploy, não a primeira requisição do primeiro usuário.
- Não versione a chave de API. `ApiKey` existe nas options, mas o valor vem de user secrets ou do cofre do provedor.

## Conteúdo complementar

**1. A tradução de erro**, verificada teste por teste:

| Resposta | Exceção | O que ela carrega |
|---|---|---|
| 200 | — | `Product` desserializado |
| 400 / 422 | `SdkValidationException` | `Errors` por campo, do `ProblemDetails` |
| 401 / 403 | `SdkAuthenticationException` | `StatusCode` |
| 404 | `SdkNotFoundException` | `detail` do corpo como mensagem |
| 429 | `SdkRateLimitException` | `RetryAfter` (30s no teste) |
| 5xx | `SdkServerException` | `StatusCode`, após esgotar retries |
| DNS/conexão | `SdkTransportException` | `InnerException` = `HttpRequestException` |
| timeout interno | `SdkTransportException` | mensagem "excedeu o tempo limite" |
| cancelamento | `OperationCanceledException` | **propagada intacta** |

**2. O retry, medido**:

| Cenário | Tentativas |
|---|---:|
| 503 e depois 200 | **2** |
| 503 persistente, `MaxRetryAttempts = 3` | **4** |
| 400 / 404 / 401 | **1** |
| 503 com `MaxRetryAttempts = 0` | **1** |
| `POST` com 503 e depois 200 | **2 POSTs** |

**3. Cancelamento contra timeout** — a distinção que o SDK faz:

```csharp
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    throw;   // o CHAMADOR desistiu
}
catch (OperationCanceledException exception)
{
    // o token do chamador nao foi cancelado: estourou o timeout interno
    throw new SdkTransportException("a requisicao excedeu o tempo limite do SDK", exception);
}
```

Sem o filtro `when`, as duas situações viram a mesma coisa — e o `catch (OperationCanceledException)` de quem chama para de funcionar.

**4. `MaxRetryAttempts = 0` não é "zero tentativas" para o Polly**:

```text
ValidationException: The 'HttpRetryStrategyOptions' are invalid.
  The field MaxRetryAttempts must be between 1 and 2147483647.
```

`HttpRetryStrategyOptions` exige no mínimo 1, e passar zero lança ao **montar o pipeline** — cinco testes falharam por isso na primeira versão. Desligar retry significa **não adicionar a estratégia**:

```csharp
if (options.MaxRetryAttempts == 0)
{
    return;
}
```

**5. Testar SDK HTTP sem rede**: o `StubHttpMessageHandler` entra como `PrimaryHandler` do cliente nomeado, então o pipeline de resiliência **real** continua no caminho:

```csharp
services.Configure<HttpClientFactoryOptions>(
    ServiceCollectionExtensions.HttpClientName,
    options => options.HttpMessageHandlerBuilderActions.Add(
        builder => builder.PrimaryHandler = handler));
```

É o que permite afirmar "fez 4 tentativas" em vez de "tem retry configurado".

Relação com os vizinhos: `MySimpleSdk` cobre a estrutura básica de um SDK com demo e testes. `ScalarDocumentationSdk` embala configuração de OpenAPI. Aqui o assunto é resiliência e contrato de erro. A trilha `08-ArchitecturalPatterns` trata de circuit breaker do lado do serviço.

## Referências e documentação complementar

- https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory
- https://learn.microsoft.com/dotnet/core/resilience/http-resilience
- https://www.pollydocs.org/strategies/retry.html
- https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap#cancellation
