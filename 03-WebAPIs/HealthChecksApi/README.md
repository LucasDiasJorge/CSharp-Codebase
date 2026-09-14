# HealthChecksApi

API que separa sondas de liveness e readiness, verifica dependências externas com health checks próprios e expõe resultados no formato que um orquestrador espera.

## Visão geral

As duas sondas respondem a perguntas diferentes, e confundi-las é o erro mais caro desta área. Liveness pergunta *o processo ainda está vivo?* — se a resposta for não, o orquestrador reinicia o container. Readiness pergunta *esta instância pode receber tráfego agora?* — se a resposta for não, a instância sai do balanceador, mas continua de pé e pode voltar sozinha.

A consequência prática aparece assim que o banco cai. Se liveness verificasse o banco, todas as instâncias falhariam a sonda ao mesmo tempo e seriam reiniciadas em massa: uma indisponibilidade parcial viraria total, e o reinício não conserta banco nenhum. Por isso a sonda de liveness deste exemplo não consulta dependência alguma.

O estado das três dependências simuladas — banco, broker e cache — é alternável por HTTP. Dá para derrubar o banco e ver readiness virar 503 enquanto liveness segue em 200, ou deixar o cache lento e ver o resultado `Degraded` que continua recebendo tráfego.

A terceira distinção é entre dependência crítica e não crítica. Sem banco a aplicação não atende nada; sem cache ela fica lenta, mas atende. Por isso o cache é registrado com `failureStatus: Degraded`, e o resultado degradado mapeia para 200.

## Conceitos abordados

- Liveness, readiness e por que uma não pode ser cópia da outra.
- Tags como critério de seleção de checks por sonda, via `Predicate`.
- `IHealthCheck` próprio para cada dependência, com dados de diagnóstico em `data`.
- `HealthStatus.Degraded` para dependência não crítica.
- `failureStatus` no registro e `context.Registration.FailureStatus` no check.
- `ResultStatusCodes` para escolher qual status HTTP cada resultado produz.
- `timeout` por check e o cancelamento que ele provoca.
- Orçamento de latência: responder devagar também é sintoma.
- `ResponseWriter` customizado para produzir JSON de diagnóstico.

## Objetivos de aprendizagem

- Decidir o que entra em cada sonda e justificar pela consequência no orquestrador.
- Classificar dependências entre críticas e não críticas antes de escrever o check.
- Escrever um `IHealthCheck` que distingue indisponibilidade de lentidão.
- Escolher o status HTTP de cada resultado em vez de aceitar o padrão sem pensar.
- Evitar que uma sonda vire vetor de sobrecarga ou de vazamento de informação.

## Estrutura do projeto

```text
HealthChecksApi/
|-- Checks/
|   |-- CacheHealthCheck.cs
|   |-- DatabaseHealthCheck.cs
|   `-- MessageBrokerHealthCheck.cs
|-- Dependencies/
|   |-- DependencySimulator.cs
|   `-- DependencyState.cs
|-- Reporting/
|   `-- HealthReportWriter.cs
|-- Properties/
|   `-- launchSettings.json
|-- HealthChecksApi.csproj
|-- HealthChecksApi.http
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 03-WebAPIs/HealthChecksApi/HealthChecksApi.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 03-WebAPIs/HealthChecksApi/HealthChecksApi.csproj
```

A API sobe em `http://localhost:5178` e não exige serviço externo — as dependências são simuladas. Requisições prontas em `HealthChecksApi.http`.

O roteiro que mostra a diferença entre as sondas:

```bash
curl -X POST "http://localhost:5178/simulation/database?state=Down"
curl -o /dev/null -w "live=%{http_code}\n"  http://localhost:5178/health/live
curl -o /dev/null -w "ready=%{http_code}\n" http://localhost:5178/health/ready
```

A saída é `live=200` e `ready=503`: o processo está vivo, mas não pode atender.

## Boas práticas e pontos de atenção

- Nunca coloque dependência externa na sonda de liveness. Uma queda de banco reiniciaria todas as instâncias ao mesmo tempo, transformando indisponibilidade parcial em total — e o reinício não conserta a dependência.
- Classifique cada dependência antes de escrever o check. Crítica derruba readiness; não crítica apenas degrada. Sem essa distinção, perder o cache tira a aplicação inteira do ar.
- Use `context.Registration.FailureStatus` em vez de `HealthCheckResult.Unhealthy()` fixo. Assim o peso da falha fica na composição, e a mesma classe de check serve para dependência crítica e opcional.
- Configure `timeout` em todo check que sai do processo. Sem ele, uma dependência pendurada segura a resposta da sonda, e o orquestrador acaba agindo por timeout próprio, sem saber o motivo.
- Trate lentidão como sintoma. O check de cache compara a latência com um orçamento e degrada antes da queda; um check que só pergunta "respondeu?" avisa tarde demais.
- Não devolva exceção nem stack trace no corpo da sonda. `/health` costuma ficar acessível para a rede interna inteira; o detalhe vai para o log, a descrição vai para a resposta.
- Health check faz trabalho de verdade a cada chamada. Sondas de segundo a segundo multiplicam esse custo por instância; para dependências caras, use cache de resultado ou `AddCheck` com intervalo maior.
- `Degraded` mapeia para 200 por padrão, e aqui isso é intencional. Se a sua semântica for outra, ajuste `ResultStatusCodes` explicitamente em vez de confiar no padrão.
- Em produção, exponha `/health` completo em porta ou rede separada das sondas, ou proteja o endpoint: ele descreve a topologia interna da aplicação.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `GET /health/live` | Liveness — só o check `self`, sem dependência externa |
| `GET /health/ready` | Readiness — banco, broker e cache |
| `GET /health` | Todos os checks, para humano e painel |
| `GET /simulation` | Estado atual das dependências simuladas |
| `POST /simulation/{dependency}?state=` | Altera o estado de uma dependência |

Checks registrados:

| Check | Tag | `failureStatus` | Timeout | Papel |
|---|---|---|---|---|
| `self` | `live` | — | — | Confirma que o processo responde |
| `database` | `ready` | `Unhealthy` | 2s | Dependência crítica |
| `broker` | `ready` | `Unhealthy` | 2s | Dependência crítica |
| `cache` | `ready` | `Degraded` | 2s | Dependência não crítica |

Estados simuláveis e o caminho que cada um exercita:

| Estado | Latência | Efeito no banco | Efeito no cache |
|---|---|---|---|
| `Healthy` | 15ms | Healthy | Healthy |
| `Slow` | 1s | Healthy (dentro do timeout) | Degraded pelo orçamento de 500ms |
| `Hanging` | 5s | Unhealthy por timeout | Degraded por timeout |
| `Down` | — | Unhealthy | Degraded |

Resultado observado ao derrubar o banco:

```text
GET /health/live   -> 200  {"status":"Healthy"}
GET /health/ready  -> 503  {"status":"Unhealthy", database: "Nao foi possivel conectar em 'database'."}
```

Resultado observado com o cache fora do ar:

```text
GET /health/ready  -> 200  {"status":"Degraded"}
```

A instância continua no balanceador de propósito: perder o cache deixa a aplicação lenta, tirar todas as instâncias do ar a deixa indisponível.

Correspondência com as sondas do Kubernetes:

| Sonda | Rota | Falha significa |
|---|---|---|
| `livenessProbe` | `/health/live` | Reiniciar o container |
| `readinessProbe` | `/health/ready` | Tirar do balanceador, sem reiniciar |
| `startupProbe` | `/health/ready` com limite maior | Ainda subindo; adia as outras sondas |

O `startupProbe` não tem equivalente separado no ASP.NET Core: reutiliza-se a rota de readiness com um número maior de tentativas, para que uma inicialização lenta não seja confundida com falha.

Relação com os vizinhos da trilha: nenhum outro projeto trata de observabilidade operacional. `ProblemDetailsApi` padroniza erros de negócio para o cliente; aqui o público é o orquestrador, e o "erro" é sobre a própria instância.

## Referências e documentação complementar

- https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
- https://learn.microsoft.com/dotnet/api/microsoft.extensions.diagnostics.healthchecks.ihealthcheck
- https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/
