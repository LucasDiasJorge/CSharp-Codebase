# 18 ideias de projetos didáticos em C#

Esta lista propõe novos samples alinhados às 13 trilhas temáticas do repositório. Os nomes são sugestões para as pastas dos projetos, e cada item explicita o principal aprendizado esperado sem repetir o foco central dos exemplos já catalogados.

Ideias já implementadas saem desta lista. As trilhas `01-Fundamentals`, `02-AsyncAndConcurrency`, `03-WebAPIs`, `04-Authentication`, `05-Messaging`, `06-Caching`, `07-DesignPatterns` e `08-ArchitecturalPatterns` não têm mais pendências — restam as trilhas de dados, algoritmos, utilitários, testes e SDKs.

## 09-Data

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 1 | `EfCoreRelationshipsDemo` | Mapear relacionamentos um-para-um, um-para-muitos e muitos-para-muitos, explorando tracking, owned types e carregamento de dados. |
| 2 | `EfCoreOptimisticConcurrencyDemo` | Simular atualizações concorrentes com concurrency tokens e ensinar estratégias de detecção, resolução e retry de conflitos. |
| 3 | `DatabaseMigrationsDemo` | Ensinar criação, aplicação e reversão de migrations, seed de dados e evolução compatível do schema. |
| 4 | `MultiTenantDataIsolationDemo` | Implementar isolamento por tenant com query filters e índices compostos, destacando riscos de vazamento entre clientes. |

## 10-Algorithms

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 5 | `BinarySearchBoundariesDemo` | Derivar busca binária por invariantes e implementar exact match, lower bound e upper bound evitando erros de índice. |
| 6 | `TrieAutocompleteDemo` | Construir uma trie para autocomplete, analisar custo por comprimento da palavra e comparar memória e busca com um dicionário. |
| 7 | `LruCacheDataStructureDemo` | Combinar dicionário e lista duplamente ligada para obter leitura, escrita e remoção em tempo constante. |
| 8 | `DynamicProgrammingCoinChangeDemo` | Resolver coin change com recursão, memoization e tabulation, comparando subproblemas repetidos e complexidade. |

## 11-Utilities

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 9 | `CsvStreamingProcessor` | Processar arquivos CSV grandes sem carregá-los por inteiro, tratando cultura, registros inválidos e escrita incremental. |
| 10 | `FileChecksumDeduplicator` | Calcular hashes por stream, localizar arquivos duplicados e discutir colisões, buffers e custo de I/O. |
| 11 | `ConfigurationOptionsDemo` | Combinar `appsettings`, variáveis de ambiente e user secrets com Options Pattern, validação no startup e reload de configuração. |

## 12-Testing

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 12 | `TestDoublesDemo` | Diferenciar dummy, stub, spy, mock e fake por exemplos, escolhendo entre verificação de estado e de comportamento. |
| 13 | `PropertyBasedTestingDemo` | Gerar entradas automaticamente, expressar invariantes e usar shrinking para encontrar o menor caso que viola uma regra. |
| 14 | `WebApiIntegrationTestingDemo` | Testar uma API em memória com `WebApplicationFactory`, substituir dependências e validar o contrato HTTP de ponta a ponta. |
| 15 | `MutationTestingDemo` | Usar mutation testing para avaliar a força da suíte e identificar testes que executam código sem verificar seu comportamento. |

## 13-SDKsAndLibraries

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 16 | `ResilientHttpSdk` | Projetar um SDK HTTP tipado com `HttpClientFactory`, configuração por options, cancelamento, retries e tradução consistente de erros. |
| 17 | `NuGetPackageLifecycleDemo` | Ensinar metadados de pacote, SemVer, `dotnet pack`, símbolos, documentação XML, feed local e consumo por outro projeto. |
| 18 | `IncrementalSourceGeneratorDemo` | Criar um source generator incremental com Roslyn, inspecionar o código gerado e testar entradas, diagnósticos e saídas. |

## Critérios usados na seleção

- Complementar os projetos existentes em vez de recriar seus exemplos centrais.
- Manter cada sample focado em um conceito principal e executável de forma independente.
- Misturar fundamentos, práticas de produção e tópicos avançados em uma progressão de aprendizado.
- Priorizar lacunas nas trilhas com menor quantidade de projetos, especialmente Testing e SDKsAndLibraries.
