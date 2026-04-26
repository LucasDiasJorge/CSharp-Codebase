# Ideias de 50 projetos C# alinhados ao CSharp-101

## Criterio de curadoria
- Baseado no indice atual do README raiz (13 trilhas + tools).
- Sem repetir os exemplos ja existentes.
- Foco em projetos didaticos, pequenos e executaveis de forma isolada.

## Lista de 50 projetos sugeridos

| # | Trilha sugerida | Nome do projeto | Objetivo de aprendizado |
|---|------------------|-----------------|-------------------------|
| 1 | 01-Fundamentals | RecordsAndValueObjectsDemo | Comparar classes, records e value objects no dominio. |
| 2 | 01-Fundamentals | PatternMatchingDeepDive | Explorar pattern matching com switch expressions e guard clauses. |
| 3 | 01-Fundamentals | NullableReferenceTypesLab | Praticar nullable reference types e eliminacao de null bugs. |
| 4 | 01-Fundamentals | CollectionExpressionsPlayground | Usar collection expressions e simplificar inicializacao de colecoes. |
| 5 | 02-AsyncAndConcurrency | ChannelProducerConsumer | Implementar produtor/consumidor com System.Threading.Channels. |
| 6 | 02-AsyncAndConcurrency | RateLimitedTaskRunner | Criar executor de tarefas com limite de concorrencia e throughput. |
| 7 | 02-AsyncAndConcurrency | CancellationTokenPatterns | Demonstrar cancelamento cooperativo em pipelines assincronos. |
| 8 | 02-AsyncAndConcurrency | AsyncStreamProcessing | Processar dados com IAsyncEnumerable e backpressure simples. |
| 9 | 03-WebAPIs | ApiVersioningPlayground | Expor multiplas versoes de API e gerenciar compatibilidade. |
| 10 | 03-WebAPIs | ProblemDetailsStandardApi | Padronizar erros HTTP com ProblemDetails e contrato consistente. |
| 11 | 03-WebAPIs | HealthChecksAndReadiness | Implementar liveness/readiness para observabilidade de servico. |
| 12 | 03-WebAPIs | OpenTelemetryTracingApi | Instrumentar API com traces, metrics e correlation ids. |
| 13 | 03-WebAPIs | FeatureFlagsApi | Aplicar feature flags em endpoints para rollout progressivo. |
| 14 | 03-WebAPIs | FileUploadSecurityApi | Tratar upload seguro com validacao, tamanho maximo e scanning mock. |
| 15 | 03-WebAPIs | MultiTenantMinimalApi | Implementar multitenancy simples por header e isolamento logico. |
| 16 | 04-Authentication | JwtRefreshTokenApi | Implementar refresh token rotativo com revogacao e expiracao. |
| 17 | 04-Authentication | ApiKeyAndHmacAuth | Comparar autenticacao por API Key e assinatura HMAC. |
| 18 | 04-Authentication | RoleAndPolicyAuthorization | Praticar autorizacao por roles e policies customizadas. |
| 19 | 04-Authentication | IdentityLockoutAndMfa | Demonstrar lockout, MFA simulada e protecao contra brute force. |
| 20 | 05-Messaging | OutboxPatternDemo | Garantir consistencia entre banco e evento com outbox pattern. |
| 21 | 05-Messaging | DeadLetterQueueHandling | Processar dead letters e politicas de retentativa. |
| 22 | 05-Messaging | IdempotentConsumerDemo | Criar consumidor idempotente com deduplicacao de mensagens. |
| 23 | 05-Messaging | MessageOrderingStrategies | Estudar ordenacao por chave, particao e reprocessamento. |
| 24 | 06-Caching | DistributedCacheStampede | Evitar cache stampede com lock distribuido e expiracao escalonada. |
| 25 | 06-Caching | TwoLevelCacheDemo | Combinar cache local em memoria com cache distribuido. |
| 26 | 06-Caching | CacheInvalidationByDomainEvent | Invalidar cache por eventos de dominio publicados em fila. |
| 27 | 06-Caching | OutputCachingApi | Aplicar output caching para endpoints de leitura intensiva. |
| 28 | 07-DesignPatterns | SpecificationPatternCatalog | Filtrar regras complexas com specification pattern reutilizavel. |
| 29 | 07-DesignPatterns | TemplateMethodPipeline | Organizar fluxos variaveis com template method. |
| 30 | 07-DesignPatterns | ObserverWithDomainEvents | Usar observer para notificar handlers de eventos de dominio. |
| 31 | 07-DesignPatterns | DecoratorForValidationAndLogging | Encadear validacao e logging com decorators composiveis. |
| 32 | 07-DesignPatterns | AntiCorruptionLayerDemo | Isolar integracao externa com anti-corruption layer. |
| 33 | 08-ArchitecturalPatterns | CleanArchitectureSample | Estruturar camadas de dominio, aplicacao e infraestrutura. |
| 34 | 08-ArchitecturalPatterns | EventDrivenInventoryFlow | Simular fluxo de estoque orientado a eventos. |
| 35 | 08-ArchitecturalPatterns | HexagonalArchitectureApi | Separar portas e adaptadores em API orientada a casos de uso. |
| 36 | 08-ArchitecturalPatterns | BffGatewayExample | Criar BFF para consolidar chamadas de multiplos servicos. |
| 37 | 09-Data | EfCoreMigrationsAndSeed | Praticar migrations, seed e rollback com EF Core. |
| 38 | 09-Data | TransactionIsolationPlayground | Comparar niveis de isolamento e efeitos de concorrencia. |
| 39 | 09-Data | SoftDeleteAndAuditTrail | Implementar soft delete com trilha de auditoria automatica. |
| 40 | 09-Data | RepositoryVsQueryObject | Contrastar repositorio tradicional e query object para leitura. |
| 41 | 10-Algorithms | TopKFrequentElements | Resolver problema Top-K com heap e analise de complexidade. |
| 42 | 10-Algorithms | ConsistentHashingDemo | Implementar consistent hashing para distribuicao de carga. |
| 43 | 10-Algorithms | ShortestPathDijkstraDemo | Aplicar Dijkstra em grafo ponderado com casos reais. |
| 44 | 11-Utilities | CsvImportExportToolkit | Criar utilitario para importacao/exportacao CSV com validacao. |
| 45 | 11-Utilities | RetryAndBackoffUtility | Empacotar politicas de retry e exponential backoff reutilizaveis. |
| 46 | 11-Utilities | HtmlToPdfAndTemplateEngine | Gerar PDF a partir de template HTML com dados dinamicos. |
| 47 | 12-Testing | ContractTestingWithPact | Criar testes de contrato para integracao entre servicos. |
| 48 | 12-Testing | TestcontainersIntegrationDemo | Rodar testes de integracao com bancos em containers efemeros. |
| 49 | 13-SDKsAndLibraries | ResilientHttpSdk | Construir SDK HTTP com retry, circuit breaker e telemetry. |
| 50 | 13-SDKsAndLibraries | PaymentsSdkWithAdapters | Criar SDK de pagamentos com adaptadores para gateways diferentes. |

## Observacao
Estas ideias podem ser priorizadas por trilha para evoluir o repositorio sem inflar a complexidade de cada projeto individual.
