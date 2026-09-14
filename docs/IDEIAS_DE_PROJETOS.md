# 37 ideias de projetos didáticos em C#

Esta lista propõe novos samples alinhados às 13 trilhas temáticas do repositório. Os nomes são sugestões para as pastas dos projetos, e cada item explicita o principal aprendizado esperado sem repetir o foco central dos exemplos já catalogados.

Ideias já implementadas saem desta lista. As trilhas `01-Fundamentals`, `02-AsyncAndConcurrency` e `03-WebAPIs` não têm mais pendências — `RecordsAndPatternMatchingDemo`, `GenericConstraintsDemo`, `SpanAndMemoryDemo` e `NullableReferenceTypesDemo` já estão no catálogo, assim como `CancellationTokenPipeline`, `ChannelProducerConsumer`, `ParallelDataProcessingDemo`, `AsyncStreamsDemo` e `AsyncLockingDemo` em `02-AsyncAndConcurrency`, e `ApiVersioningDemo`, `ProblemDetailsApi`, `ServerSentEventsDemo` e `HealthChecksApi` em `03-WebAPIs`.

## 04-Authentication

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 1 | `RefreshTokenRotationDemo` | Implementar access tokens curtos e refresh tokens rotativos, abordando revogação, armazenamento seguro e detecção de reutilização. |
| 2 | `PolicyBasedAuthorizationDemo` | Ensinar autorização baseada em policies, claims, requirements e handlers, incluindo autorização sobre um recurso específico. |
| 3 | `PasskeyAuthenticationDemo` | Apresentar o fluxo WebAuthn/FIDO2, registro de credenciais e autenticação resistente a phishing sem armazenar senhas. |
| 4 | `ApiKeyAuthenticationDemo` | Criar um authentication handler para API keys, explorando hash, rotação, escopos, revogação e comparação em tempo constante. |

## 05-Messaging

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 5 | `TransactionalOutboxDemo` | Demonstrar como persistir dados e eventos na mesma transação e publicar mensagens posteriormente sem dual write. |
| 6 | `DeadLetterQueueDemo` | Ensinar retries com limite, tratamento de poison messages, envio para dead-letter queue e reprocessamento controlado. |
| 7 | `KafkaOrderingDemo` | Explicar como chaves, partições e consumer groups influenciam ordenação, distribuição de carga e paralelismo no Kafka. |
| 8 | `RabbitMqRequestReplyDemo` | Implementar request/reply com `CorrelationId`, fila de resposta, timeout e limpeza de requisições pendentes. |

## 06-Caching

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 9 | `CacheStampedeProtectionDemo` | Simular cache stampede e aplicar single-flight, TTL com jitter e stale-while-revalidate para proteger a fonte de dados. |
| 10 | `DistributedCacheInvalidationDemo` | Sincronizar a invalidação de caches locais entre instâncias usando Redis Pub/Sub e discutir consistência eventual. |
| 11 | `RedisDistributedLockDemo` | Ensinar aquisição, renovação e liberação de locks distribuídos com token de propriedade, incluindo falhas de lease e limitações do padrão. |

## 07-DesignPatterns

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 12 | `CommandUndoRedoDemo` | Encapsular operações como comandos e implementar histórico, undo e redo, separando a solicitação de sua execução. |
| 13 | `ObserverStockAlertsDemo` | Implementar o padrão Observer com inscrição e remoção de observadores, comparando-o com events nativos do C#. |
| 14 | `MementoDocumentHistoryDemo` | Salvar e restaurar estados de um documento sem expor sua estrutura interna, discutindo memória e limites do histórico. |
| 15 | `ProxyRemoteServiceDemo` | Usar Proxy para controlar acesso a um serviço remoto, adicionando lazy loading, autorização e telemetria sem alterar o cliente. |

## 08-ArchitecturalPatterns

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 16 | `EventSourcingBankAccountDemo` | Modelar uma conta bancária por eventos, reconstruir estado, controlar versões concorrentes e introduzir snapshots. |
| 17 | `ModularMonolithCommerceDemo` | Organizar catálogo, pedidos e pagamentos em módulos com contratos explícitos, bancos logicamente isolados e eventos de integração internos. |
| 18 | `TransactionalInboxDemo` | Garantir consumo idempotente ao registrar mensagens recebidas e alterações de domínio na mesma transação. |
| 19 | `ApiGatewayAggregationDemo` | Agregar respostas de serviços, propagar correlation IDs e lidar com timeout, falha parcial e composição de contratos. |

## 09-Data

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 20 | `EfCoreRelationshipsDemo` | Mapear relacionamentos um-para-um, um-para-muitos e muitos-para-muitos, explorando tracking, owned types e carregamento de dados. |
| 21 | `EfCoreOptimisticConcurrencyDemo` | Simular atualizações concorrentes com concurrency tokens e ensinar estratégias de detecção, resolução e retry de conflitos. |
| 22 | `DatabaseMigrationsDemo` | Ensinar criação, aplicação e reversão de migrations, seed de dados e evolução compatível do schema. |
| 23 | `MultiTenantDataIsolationDemo` | Implementar isolamento por tenant com query filters e índices compostos, destacando riscos de vazamento entre clientes. |

## 10-Algorithms

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 24 | `BinarySearchBoundariesDemo` | Derivar busca binária por invariantes e implementar exact match, lower bound e upper bound evitando erros de índice. |
| 25 | `TrieAutocompleteDemo` | Construir uma trie para autocomplete, analisar custo por comprimento da palavra e comparar memória e busca com um dicionário. |
| 26 | `LruCacheDataStructureDemo` | Combinar dicionário e lista duplamente ligada para obter leitura, escrita e remoção em tempo constante. |
| 27 | `DynamicProgrammingCoinChangeDemo` | Resolver coin change com recursão, memoization e tabulation, comparando subproblemas repetidos e complexidade. |

## 11-Utilities

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 28 | `CsvStreamingProcessor` | Processar arquivos CSV grandes sem carregá-los por inteiro, tratando cultura, registros inválidos e escrita incremental. |
| 29 | `FileChecksumDeduplicator` | Calcular hashes por stream, localizar arquivos duplicados e discutir colisões, buffers e custo de I/O. |
| 30 | `ConfigurationOptionsDemo` | Combinar `appsettings`, variáveis de ambiente e user secrets com Options Pattern, validação no startup e reload de configuração. |

## 12-Testing

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 31 | `TestDoublesDemo` | Diferenciar dummy, stub, spy, mock e fake por exemplos, escolhendo entre verificação de estado e de comportamento. |
| 32 | `PropertyBasedTestingDemo` | Gerar entradas automaticamente, expressar invariantes e usar shrinking para encontrar o menor caso que viola uma regra. |
| 33 | `WebApiIntegrationTestingDemo` | Testar uma API em memória com `WebApplicationFactory`, substituir dependências e validar o contrato HTTP de ponta a ponta. |
| 34 | `MutationTestingDemo` | Usar mutation testing para avaliar a força da suíte e identificar testes que executam código sem verificar seu comportamento. |

## 13-SDKsAndLibraries

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 35 | `ResilientHttpSdk` | Projetar um SDK HTTP tipado com `HttpClientFactory`, configuração por options, cancelamento, retries e tradução consistente de erros. |
| 36 | `NuGetPackageLifecycleDemo` | Ensinar metadados de pacote, SemVer, `dotnet pack`, símbolos, documentação XML, feed local e consumo por outro projeto. |
| 37 | `IncrementalSourceGeneratorDemo` | Criar um source generator incremental com Roslyn, inspecionar o código gerado e testar entradas, diagnósticos e saídas. |

## Critérios usados na seleção

- Complementar os projetos existentes em vez de recriar seus exemplos centrais.
- Manter cada sample focado em um conceito principal e executável de forma independente.
- Misturar fundamentos, práticas de produção e tópicos avançados em uma progressão de aprendizado.
- Priorizar lacunas nas trilhas com menor quantidade de projetos, especialmente Testing e SDKsAndLibraries.
