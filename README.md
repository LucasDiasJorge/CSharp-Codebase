# CSharp-101

## Visão geral

O CSharp-101 é um repositório educacional com exemplos práticos em C# e .NET organizados por trilhas temáticas. Cada pasta reúne projetos pequenos e focados, com documentação local padronizada para facilitar leitura, execução isolada e reaproveitamento como referência técnica.

O catálogo cobre fundamentos da linguagem, APIs web, autenticação, mensageria, cache, padrões de projeto, padrões arquiteturais, persistência, algoritmos, utilitários, testes e SDKs. As convenções de documentação ficam em `docs/` e o padronizador de README em `tools/`.

## Conceitos abordados

- Linguagem C#, orientação a objetos, LINQ, delegates, eventos e reflection.
- ASP.NET Core, Minimal APIs, middleware, gRPC, autenticação, autorização e segurança.
- Mensageria, Redis, design patterns, CQRS, Saga, persistência, algoritmos e utilitários de apoio.
- Testes, benchmarks e construção de bibliotecas e SDKs reutilizáveis.

## Objetivos de aprendizagem

- Encontrar rapidamente um exemplo prático para um tema específico do ecossistema .NET.
- Executar exemplos de forma isolada, com comandos direcionados por projeto.
- Comparar abordagens diferentes para o mesmo problema técnico.
- Usar os READMEs locais como índice curto de estudo e navegação.

## Estrutura do projeto

```text
CSharp-101/
+-- 01-Fundamentals/
+-- 02-AsyncAndConcurrency/
+-- 03-WebAPIs/
+-- 04-Authentication/
+-- 05-Messaging/
+-- 06-Caching/
+-- 07-DesignPatterns/
+-- 08-ArchitecturalPatterns/
+-- 09-Data/
+-- 10-Algorithms/
+-- 11-Utilities/
+-- 12-Testing/
+-- 13-SDKsAndLibraries/
+-- docs/
+-- tools/
+-- CSharp-101.sln
+-- README.md
+\-- .github/
```

## Como executar

Prefira comandos direcionados ao arquivo `.csproj` do exemplo que deseja estudar.

```bash
# Build de um exemplo console
dotnet build 01-Fundamentals/AggregationDepartmentManagement/AggregationDepartmentManagement.csproj

# Execução de uma API
dotnet run --project 03-WebAPIs/MinimalApiDemo/MinimalApiDemo.csproj

# Execução de testes
dotnet test 12-Testing/OrderRuleConsole/OrderRuleConsole.Tests/OrderRuleConsole.Tests.csproj

# Reaplicar a padronização de documentação
powershell -ExecutionPolicy Bypass -File .\tools\Standardize-Readmes.ps1
```

## Boas práticas e pontos de atenção

- Use o README local de cada pasta antes de executar, alterar ou validar o projeto.
- Prefira `build`, `run` e `test` por `.csproj`; a solução raiz funciona melhor como índice do workspace do que como ponto único de validação.
- Exemplos com Kafka, RabbitMQ, Redis, MySQL, PostgreSQL ou MongoDB exigem serviços externos ativos.
- As convenções oficiais de documentação deste repositório estão em `docs/CONVENCOES.md` e `docs/README_TEMPLATE.md`.

## Conteúdo complementar

### Trilhas temáticas

| Pasta | Foco principal | Exemplos de referência |
|------|----------------|------------------------|
| `01-Fundamentals` | Fundamentos da linguagem e OO | `Course`, `Events`, `Linq`, `Reflection` |
| `02-AsyncAndConcurrency` | Async/await, tasks, threads e coordenação | `Asynchronous`, `AsyncTasksDemo`, `JobQueueDemo` |
| `03-WebAPIs` | APIs, middleware, gRPC e aplicações web | `MinimalApiDemo`, `GrpcSample`, `CustomMiddleware` |
| `04-Authentication` | JWT, OAuth, sessão e segurança | `Auth`, `SessionManagement`, `AdvancedAuthSystem` |
| `05-Messaging` | Filas, brokers e streaming | `Kafka`, `RabbitMQ`, `KafkaStreamApi` |
| `06-Caching` | Estratégias de cache e Redis | `CacheAside`, `CachePatterns`, `FusionCache` |
| `07-DesignPatterns` | GoF, SOLID e modelagem OO | `DesignPattern`, `SOLIDExamples`, `ObjectCalisthenics` |
| `08-ArchitecturalPatterns` | Organização de casos de uso e padrões arquiteturais | `UseCases`, `CQRSDemo`, `SagaPattern` |
| `09-Data` | Bancos de dados, ORMs e acesso a dados | `Dapper`, `MongoUserApi`, `MoneyStorageApi` |
| `10-Algorithms` | Estruturas de dados e análise de cenários | `GraphTraversalDemo`, `PriorityQueueDemo`, `SlidingWindows` |
| `11-Utilities` | Transformação de dados, serialização e observabilidade | `Serialization`, `NPOIDemo`, `SerilogExample` |
| `12-Testing` | Benchmarks e validação de comportamento | `BenchmarkTool`, `OrderRuleConsole` |
| `13-SDKsAndLibraries` | Bibliotecas e SDKs reutilizáveis | `MySimpleSdk` |

### Serviços externos comuns

```bash
# RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

# Redis
docker run -d --name redis -p 6379:6379 redis

# Kafka (exemplo da pasta Kafka)
cd 05-Messaging/Kafka
docker compose up -d
```

### Ferramentas de documentação

- `docs/CONVENCOES.md`: convenções de escrita e manutenção dos READMEs.
- `docs/README_TEMPLATE.md`: template base para novos exemplos.
- `tools/ReadmeStandardizer/`: utilitário em C# para padronização em lote.

## Referências

- [Documentação do .NET](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [Refactoring Guru](https://refactoring.guru/design-patterns)

## Documentação complementar

- [docs/CONVENCOES.md](./docs/CONVENCOES.md) - Convenções de documentação do repositório.
- [docs/README_TEMPLATE.md](./docs/README_TEMPLATE.md) - Template base para READMEs padronizados.
