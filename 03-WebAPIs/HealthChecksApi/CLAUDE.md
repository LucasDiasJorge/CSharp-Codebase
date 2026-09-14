# CLAUDE.md — HealthChecksApi

API que separa liveness de readiness, com health checks próprios para dependências simuladas e saída JSON para orquestrador. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/HealthChecksApi/HealthChecksApi.csproj
dotnet run --project 03-WebAPIs/HealthChecksApi/HealthChecksApi.csproj
```

Sobe em `http://localhost:5178`. Cenários de falha prontos em `HealthChecksApi.http`.

## Estrutura interna

`Program.cs` concentra a decisão didática: o check `self` tem tag `live` e **não consulta nada externo**; `database`, `broker` e `cache` têm tag `ready`. As sondas se distinguem pelo `Predicate` sobre as tags. `/health/ready` declara `ResultStatusCodes` explicitamente, mapeando `Degraded` para 200 de propósito.

`Dependencies/DependencySimulator` mantém o estado das três dependências e simula a latência de cada checagem, respeitando o `CancellationToken` — é ele que o `timeout` do registro cancela. `Checks/` tem um `IHealthCheck` por dependência, todos usando `context.Registration.FailureStatus` em vez de `Unhealthy()` fixo, para que o peso da falha fique na composição.

`CacheHealthCheck` é o único com orçamento de latência (500ms): demonstra degradação por lentidão, não só por queda.

`Reporting/HealthReportWriter` substitui o writer padrão, que escreve só a palavra do status.

## Pontos de atenção

- TFM `net10.0` (a maior parte da trilha é `net9.0`): a máquina não tem o runtime ASP.NET Core 9.0 instalado e este sample precisa ser executado. Mesma decisão de [ApiVersioningDemo](../ApiVersioningDemo/CLAUDE.md).
- Sem pacote externo: `Microsoft.Extensions.Diagnostics.HealthChecks` faz parte do framework compartilhado. Template `web` (Minimal API).
- **A lição central é a sonda de liveness não consultar dependência externa.** Se alguém "melhorar" o exemplo adicionando a tag `live` ao check de banco, o projeto passa a ensinar exatamente o erro que existe para evitar. O roteiro que prova o ponto (banco `Down` → `live=200`, `ready=503`) está no README e em `HealthChecksApi.http`.
- **As três latências são calibradas entre si**, não são números arbitrários: `Slow` = 1s (acima do orçamento de 500ms do cache, abaixo do timeout de 2s) e `Hanging` = 5s (acima do timeout). Mexer em qualquer um dos três sem olhar os outros torna caminhos inalcançáveis — na primeira versão deste sample, `Slow` era 3s e a degradação por orçamento do cache era código morto, porque o timeout sempre disparava antes.
- `Degraded` → 200 em `/health/ready` é escolha deliberada, não descuido. Está comentado no `Program.cs`.
- O timeout do `AddCheck` cancela o token, e cada check trata `OperationCanceledException` com `when (cancellationToken.IsCancellationRequested)` para dar a mensagem certa. Sem esse catch, a falha vira genérica e perde a causa.
- A exceção fica fora do corpo da resposta de propósito (só descrição e `data`): `/health` descreve a topologia interna e costuma ficar exposto na rede interna inteira.
- Estado em memória num singleton: reiniciar o processo devolve tudo a `Healthy`.
- **Fronteira com os vizinhos**: nenhum outro projeto da trilha trata de observabilidade operacional. `ProblemDetailsApi` padroniza erro de negócio para o cliente; aqui o público é o orquestrador.
