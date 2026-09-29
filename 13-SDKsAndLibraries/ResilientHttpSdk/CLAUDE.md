# CLAUDE.md — ResilientHttpSdk

SDK HTTP tipado com options validadas, retry seletivo, tradução de erro e suíte que exercita o pipeline real sem rede. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 13-SDKsAndLibraries/ResilientHttpSdk/src/ResilientHttpSdk/ResilientHttpSdk.csproj
dotnet test  13-SDKsAndLibraries/ResilientHttpSdk/src/ResilientHttpSdk.Tests/ResilientHttpSdk.Tests.csproj

# Ver as contagens de tentativa
dotnet test 13-SDKsAndLibraries/ResilientHttpSdk/src/ResilientHttpSdk.Tests/ResilientHttpSdk.Tests.csproj --logger "console;verbosity=detailed"
```

23 testes, todos passando (~10s, por causa do timeout real de 1s em um cenário). Layout `src/`, como `MySimpleSdk` e `ScalarDocumentationSdk`. Sem serviço externo.

## Estrutura interna

`ServiceCollectionExtensions.AddResilientHttpSdk` é a **única** porta de entrada: registra options com `ValidateOnStart`, o cliente tipado e o handler de resiliência. Duas sobrecargas (por `IConfiguration` e por lambda) compartilham `AddResilientHttpSdkCore`.

`ProductsClient` só traduz: resposta → resultado ou exceção. Retry e timeout ficam no pipeline, fora dele. **Não mover retry para dentro do cliente** — duplicaria o que o handler já faz.

`SdkExceptions.cs` tem a hierarquia. `SdkException.StatusCode` é nulo em falha de transporte, e isso é intencional.

`ResilientHttpSdk.Tests/StubHttpMessageHandler` entra como `PrimaryHandler` via `HttpClientFactoryOptions`, então o pipeline de resiliência **real** fica no caminho. `CallCount` é o instrumento dos testes de retry.

## Pontos de atenção

- TFM `net10.0` nos dois projetos. Pacotes: `Microsoft.Extensions.Http.Resilience` e `Microsoft.Extensions.Options.DataAnnotations` na biblioteca.
- **`MaxRetryAttempts = 0` não é aceito por `HttpRetryStrategyOptions`** (mínimo 1) e lança `ValidationException` ao montar o pipeline. Cinco dos 23 testes falharam por isso na primeira versão. A solução é o `return` antes do `AddRetry` quando o valor é zero. **Não remover esse early return.**
- O filtro `when (cancellationToken.IsCancellationRequested)` em `ProductsClient.SendAsync` é o que separa cancelamento do chamador de timeout interno — as duas situações chegam como a mesma `OperationCanceledException`. Remover o filtro quebra `CancellationTests` e, o que é pior, o `catch` de quem consome.
- `EnsureTrailingSlash` existe porque `BaseAddress` sem `/` final faz o último segmento ser substituído. Há teste fixando `https://api.exemplo.com/v2` + `products/p-1`.
- `ShouldRetry` lista os status explicitamente. Acrescentar 400 ou 404 ali transforma erro de cliente em quatro chamadas inúteis.
- O teste `RetryEmPost_ReenviaACriacao` **afirma** o comportamento arriscado (2 POSTs) em vez de esconder. É didático; não "consertar" desligando retry no POST sem atualizar README e teste.
- `BaseDelayMilliseconds` é 1 nos testes (`SdkTestHost`); o padrão da biblioteca é 200. Subir nos testes faz a suíte levar segundos.
- `TimeoutInterno_ViraSdkTransportException` usa timeout real de 1s — é o teste mais lento da suíte.
- **Fronteira com os vizinhos**: `MySimpleSdk` cobre estrutura básica de SDK; `ScalarDocumentationSdk` cobre empacotar configuração. Não adicionar circuit breaker aqui (está em `08-ArchitecturalPatterns`) nem autenticação OAuth (está em `04-Authentication`).
