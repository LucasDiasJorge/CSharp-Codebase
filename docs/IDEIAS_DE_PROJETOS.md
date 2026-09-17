# 28 ideias de projetos didáticos em C#

Esta lista propõe novos samples alinhados às 13 trilhas temáticas do repositório. Os nomes são sugestões para as pastas dos projetos, e cada item explicita o principal aprendizado esperado sem repetir o foco central dos exemplos já catalogados.

Ideias já implementadas saem desta lista. As trilhas `01-Fundamentals`, `02-AsyncAndConcurrency`, `03-WebAPIs`, `04-Authentication` e `05-Messaging` não têm mais pendências — além dos samples já catalogados nas quatro primeiras, `TransactionalOutboxDemo`, `DeadLetterQueueDemo`, `KafkaOrderingDemo` e `RabbitMqRequestReplyDemo` cobrem a trilha de mensageria.

## 06-Caching

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 1 | `DistributedCacheInvalidationDemo` | Sincronizar a invalidação de caches locais entre instâncias usando Redis Pub/Sub e discutir consistência eventual. |
| 2 | `RedisDistributedLockDemo` | Ensinar aquisição, renovação e liberação de locks distribuídos com token de propriedade, incluindo falhas de lease e limitações do padrão. |

## 07-DesignPatterns

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 3 | `CommandUndoRedoDemo` | Encapsular operações como comandos e implementar histórico, undo e redo, separando a solicitação de sua execução. |
| 4 | `ObserverStockAlertsDemo` | Implementar o padrão Observer com inscrição e remoção de observadores, comparando-o com events nativos do C#. |
| 5 | `MementoDocumentHistoryDemo` | Salvar e restaurar estados de um documento sem expor sua estrutura interna, discutindo memória e limites do histórico. |
| 6 | `ProxyRemoteServiceDemo` | Usar Proxy para controlar acesso a um serviço remoto, adicionando lazy loading, autorização e telemetria sem alterar o cliente. |

## 08-ArchitecturalPatterns

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 7 | `EventSourcingBankAccountDemo` | Modelar uma conta bancária por eventos, reconstruir estado, controlar versões concorrentes e introduzir snapshots. |
| 8 | `ModularMonolithCommerceDemo` | Organizar catálogo, pedidos e pagamentos em módulos com contratos explícitos, bancos logicamente isolados e eventos de integração internos. |
| 9 | `TransactionalInboxDemo` | Garantir consumo idempotente ao registrar mensagens recebidas e alterações de domínio na mesma transação. |
| 10 | `ApiGatewayAggregationDemo` | Agregar respostas de serviços, propagar correlation IDs e lidar com timeout, falha parcial e composição de contratos. |

## 09-Data

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 11 | `EfCoreRelationshipsDemo` | Mapear relacionamentos um-para-um, um-para-muitos e muitos-para-muitos, explorando tracking, owned types e carregamento de dados. |
| 12 | `EfCoreOptimisticConcurrencyDemo` | Simular atualizações concorrentes com concurrency tokens e ensinar estratégias de detecção, resolução e retry de conflitos. |
| 13 | `DatabaseMigrationsDemo` | Ensinar criação, aplicação e reversão de migrations, seed de dados e evolução compatível do schema. |
| 14 | `MultiTenantDataIsolationDemo` | Implementar isolamento por tenant com query filters e índices compostos, destacando riscos de vazamento entre clientes. |

## 10-Algorithms

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 15 | `BinarySearchBoundariesDemo` | Derivar busca binária por invariantes e implementar exact match, lower bound e upper bound evitando erros de índice. |
| 16 | `TrieAutocompleteDemo` | Construir uma trie para autocomplete, analisar custo por comprimento da palavra e comparar memória e busca com um dicionário. |
| 17 | `LruCacheDataStructureDemo` | Combinar dicionário e lista duplamente ligada para obter leitura, escrita e remoção em tempo constante. |
| 18 | `DynamicProgrammingCoinChangeDemo` | Resolver coin change com recursão, memoization e tabulation, comparando subproblemas repetidos e complexidade. |

## 11-Utilities

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 19 | `CsvStreamingProcessor` | Processar arquivos CSV grandes sem carregá-los por inteiro, tratando cultura, registros inválidos e escrita incremental. |
| 20 | `FileChecksumDeduplicator` | Calcular hashes por stream, localizar arquivos duplicados e discutir colisões, buffers e custo de I/O. |
| 21 | `ConfigurationOptionsDemo` | Combinar `appsettings`, variáveis de ambiente e user secrets com Options Pattern, validação no startup e reload de configuração. |

## 12-Testing

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 22 | `TestDoublesDemo` | Diferenciar dummy, stub, spy, mock e fake por exemplos, escolhendo entre verificação de estado e de comportamento. |
| 23 | `PropertyBasedTestingDemo` | Gerar entradas automaticamente, expressar invariantes e usar shrinking para encontrar o menor caso que viola uma regra. |
| 24 | `WebApiIntegrationTestingDemo` | Testar uma API em memória com `WebApplicationFactory`, substituir dependências e validar o contrato HTTP de ponta a ponta. |
| 25 | `MutationTestingDemo` | Usar mutation testing para avaliar a força da suíte e identificar testes que executam código sem verificar seu comportamento. |

## 13-SDKsAndLibraries

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 26 | `ResilientHttpSdk` | Projetar um SDK HTTP tipado com `HttpClientFactory`, configuração por options, cancelamento, retries e tradução consistente de erros. |
| 27 | `NuGetPackageLifecycleDemo` | Ensinar metadados de pacote, SemVer, `dotnet pack`, símbolos, documentação XML, feed local e consumo por outro projeto. |
| 28 | `IncrementalSourceGeneratorDemo` | Criar um source generator incremental com Roslyn, inspecionar o código gerado e testar entradas, diagnósticos e saídas. |

## Critérios usados na seleção

- Complementar os projetos existentes em vez de recriar seus exemplos centrais.
- Manter cada sample focado em um conceito principal e executável de forma independente.
- Misturar fundamentos, práticas de produção e tópicos avançados em uma progressão de aprendizado.
- Priorizar lacunas nas trilhas com menor quantidade de projetos, especialmente Testing e SDKsAndLibraries.
