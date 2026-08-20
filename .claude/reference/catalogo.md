# Referência do catálogo

Fatos verificáveis sobre o CSharp-Codebase, compartilhados pelas skills e agentes de `.claude/`.
Regras de conduta ficam em [CLAUDE.md](../../CLAUDE.md); este arquivo é **dado**, não política.

Números conferidos em 2026-08-19. Quando divergirem do checkout, o checkout vence — reconfira com
os comandos da seção "Comandos de verificação" antes de reportar.

## Taxonomia das trilhas

Escolha a trilha pelo **conceito principal ensinado**, não pelo template do projeto.
Em empate, prefira a mais específica.

| Trilha | Conceito principal | Projetos |
|---|---|---|
| `01-Fundamentals` | Sintaxe, OOP, LINQ, delegates, eventos, reflection | 15 |
| `02-AsyncAndConcurrency` | async/await, tasks, threads, sincronização | 8 |
| `03-WebAPIs` | REST, Minimal API, gRPC, middleware, filtros | 17 |
| `04-Authentication` | Autenticação, autorização, JWT, OAuth, sessão | 5 |
| `05-Messaging` | Kafka, RabbitMQ, filas e eventos | 4 |
| `06-Caching` | Redis, padrões de cache, cache distribuído | 12 |
| `07-DesignPatterns` | GoF, SOLID, code smells, DDD tático | 8 |
| `08-ArchitecturalPatterns` | CQRS, Saga, circuit breaker, use cases | 7 |
| `09-Data` | EF Core, Dapper, SQL, NoSQL, persistência | 12 |
| `10-Algorithms` | Algoritmos e estruturas de dados | 7 |
| `11-Utilities` | Serialização, arquivos, observabilidade | 12 |
| `12-Testing` | Testes, benchmark, validação de regras | 3 |
| `13-SDKsAndLibraries` | SDKs, packages, bibliotecas reutilizáveis | 4 |
| `tools/` | Utilitários do próprio repositório | 1 |

Os contadores acima são os declarados no título de cada categoria no README raiz; a skill
`auditar-catalogo` compara com a contagem real em disco.

## Localizar o `.csproj` real

O layout **não** é uniforme. Nunca monte um comando presumindo `Trilha/Projeto/Projeto.csproj`.

```bash
find <trilha>/<Projeto> -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"
```

Casos que quebram a suposição:

- **Layout `src/`** — 8 projetos:
  `02-AsyncAndConcurrency/AtomicOperationsDemo/src/AtomicOperationsDemo`,
  `03-WebAPIs/GrpcSample/src/{GrpcSample.Client,GrpcSample.Contracts,GrpcSample.Server}`,
  `06-Caching/UnifiedCacheSdk/src/UnifiedCacheSdk`,
  `13-SDKsAndLibraries/MySimpleSdk/src/{MySimpleSdk,MySimpleSdk.Demo,MySimpleSdk.Tests}`.
- **Pasta agrupadora com vários `.csproj`** — `05-Messaging/Kafka/{Send,Receive}`,
  `05-Messaging/RabbitMQ/{Send,Receive}`, `06-Caching/Caching/*`, `09-Data/Data/*`,
  `07-DesignPatterns/ObjectCalisthenics/{BadOrderApi,GoodOrderApi}`,
  `11-Utilities/TaskScheduler/{production,project}`.
- **Nome do `.csproj` diferente da pasta pai** — `01-Fundamentals/Sockets/*`,
  `11-Utilities/SerilogExample/SerilogExample`.

## Target frameworks

TFMs são heterogêneos por projeto. **Preserve o TFM existente ao editar**; não modernize sem pedido.

| TFM | Projetos |
|---|---|
| `net9.0` | 81 |
| `net10.0` | 13 |
| `net8.0` | 12 |
| `net7.0` | 2 |
| `net6.0` | 2 |
| `net5.0` | 2 |
| `net10.0-windows` | 2 |
| `netstandard2.0` | 1 |

SDK instalado: **10.0.400** (`dotnet --version`). TFMs `net5.0`, `net6.0` e `net7.0` estão fora
de suporte — o build pode falhar por runtime/targeting pack ausente. Isso é baseline, não
regressão introduzida por uma alteração.

## Projetos com build quebrado (baseline conhecido)

Não conte estas falhas como regressão da sua alteração — e não as conserte de passagem sem que o
pedido inclua isso.

### `NETSDK1013` — 10 projetos sem `<TargetFramework>`

`Directory.Build.props` foi removido no commit `50763d5`, mas estes `.csproj` ainda têm apenas um
**comentário** dizendo que herdam o TFM. O elemento real não existe:

| Projeto | TFM correto |
|---|---|
| `01-Fundamentals/LogicalOperatorsDemo` | `net9.0` |
| `02-AsyncAndConcurrency/JobQueueDemo` | `net9.0` |
| `04-Authentication/AdvancedAuthSystem` | `net9.0` |
| `04-Authentication/SessionManagement` | `net9.0` |
| `07-DesignPatterns/SOLIDExamples` | `net9.0` |
| `10-Algorithms/GraphTraversalDemo` | `net9.0` |
| `06-Caching/Caching/CacheAside` | `net8.0` (pacotes 8.0.x) |
| `06-Caching/Caching/CacheIncrement` | `net8.0` (pacotes 8.0.x) |
| `06-Caching/Caching/CachePatterns` | `net8.0` (pacotes 8.0.x) |
| `09-Data/Data/MongoUserApi` | `net8.0` (pacotes 8.0.x) |

Duas correções possíveis: restaurar `Directory.Build.props` na raiz (conserta os 10 de uma vez,
mas impõe um TFM único) ou declarar `TargetFramework`, `Nullable` e `ImplicitUsings` em cada
`.csproj` (preserva a diferença net9.0/net8.0). A segunda respeita a heterogeneidade real.

### `CS0246` — `MySimpleSdk.Tests`

`13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests` não tem `ProjectReference` para
`../MySimpleSdk/MySimpleSdk.csproj`, então `SdkClient` e `SdkService` não resolvem.

## Projetos fora da `CSharp-Codebase.sln`

116 dos 125 `.csproj` estão registrados. Fora da solução:

```text
02-AsyncAndConcurrency/TaskWhenAll/example
04-Authentication/SessionManagement
06-Caching/Caching/RedisMetaData
07-DesignPatterns/DesignPattern/Creational/Registry
07-DesignPatterns/PortsAndAdapters/example
10-Algorithms/GraphTraversalDemo
10-Algorithms/Two-Sum
11-Utilities/TaskScheduler/production
11-Utilities/TaskScheduler/project
tools/ReadmeStandardizer
```

A `.sln` é índice do workspace, não unidade de build — a ausência não quebra o build, mas o
projeto some do `dotnet sln list`. Registre projetos **novos**; regularizar os antigos é tarefa
própria, não trabalho de passagem.

## Serviços externos por projeto

Mapeado pelos pacotes NuGet declarados nos `.csproj`.

| Serviço | Projetos |
|---|---|
| **Kafka** | `05-Messaging/Kafka/{Send,Receive}`, `05-Messaging/KafkaStreamApi` |
| **RabbitMQ** | `05-Messaging/RabbitMQ/{Send,Receive}` |
| **Redis** | `02-AsyncAndConcurrency/AtomicOperationsDemo`, `04-Authentication/SessionManagement`, `06-Caching/Caching/{CacheIncrement,RedisConsoleApp,RedisHashFieldExpire,RedisMetaData,RedisMySQLIntegration}`, `06-Caching/UnifiedCacheSdk` |
| **MySQL** | `06-Caching/Caching/{CacheIncrement,RedisMySQLIntegration}`, `09-Data/Data/{MoneyStorageApi,MysqlExample,ProcedureExample}` |
| **PostgreSQL** | `02-AsyncAndConcurrency/TaskManagement`, `03-WebAPIs/WebApplication/MyAPI`, `09-Data/Data/Postgres`, `11-Utilities/ClassToDTO` |
| **MongoDB** | `09-Data/Data/MongoUserApi` |
| **SQLite** | `02-AsyncAndConcurrency/AtomicOperationsDemo`, `03-WebAPIs/TransactionalOrderApi`, `09-Data/Data/sqlite-sample-api` — arquivo local, **não** exige serviço |

`docker-compose.yml` existe em `05-Messaging/Kafka/` e `06-Caching/Caching/CacheIncrement/`.
Para os demais, use os `docker run` do README raiz. Ver `.claude/skills/subir-servicos/`.

## Projetos de teste

Só existem 3, todos xUnit:

| Projeto | TFM |
|---|---|
| `12-Testing/OrderRuleConsole/OrderRuleConsole.Tests` | `net9.0` |
| `09-Data/Data/sqlite-sample-api.Tests` | `net6.0` |
| `13-SDKsAndLibraries/MySimpleSdk/src/MySimpleSdk.Tests` | `net5.0` (não compila, ver acima) |

## Documentação: onde cada coisa mora

| Arquivo | Papel |
|---|---|
| `docs/CONVENCOES.md` | Idioma, ordem das seções, regras de comando, manutenção |
| `docs/README_TEMPLATE.md` | Template literal do README de projeto |
| `README.md` (raiz) | Catálogo: índice por categoria, contadores, trilhas temáticas |
| `<projeto>/README.md` | README local, 9 seções na ordem fixa |
| `<projeto>/CLAUDE.md` | Comando exato, arquitetura interna, armadilhas do projeto |
| `.github/instructions/*.instructions.md` | Regras por escopo de arquivo (`applyTo`) |
| `.github/skills/create-csharp-project-by-task/SKILL.md` | Processo canônico de criação de sample |

## Comandos de verificação

```bash
# Todos os .csproj reais
find . -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"

# .csproj sem <TargetFramework> real (comentário não conta)
for f in $(find . -name "*.csproj" -not -path "*/bin/*" -not -path "*/obj/*"); do
  grep -q "<TargetFramework" "$f" || echo "$f"
done

# Projetos ausentes da solução
dotnet sln CSharp-Codebase.sln list

# Ocorrências de var (heurística — revisar manualmente, pega comentários e strings)
grep -rn --include="*.cs" -E "(^|[^A-Za-z0-9_])var[[:space:]]+[a-zA-Z_]" . \
  | grep -v "/bin/" | grep -v "/obj/"
```

Sempre exclua `bin/` e `obj/` de buscas e diffs.
