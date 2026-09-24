# 7 ideias de projetos didáticos em C#

Esta lista propõe novos samples alinhados às 13 trilhas temáticas do repositório. Os nomes são sugestões para as pastas dos projetos, e cada item explicita o principal aprendizado esperado sem repetir o foco central dos exemplos já catalogados.

Ideias já implementadas saem desta lista. As trilhas `01-Fundamentals` a `11-Utilities` não têm mais pendências — restam as trilhas de testes e SDKs.

## 12-Testing

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 1 | `TestDoublesDemo` | Diferenciar dummy, stub, spy, mock e fake por exemplos, escolhendo entre verificação de estado e de comportamento. |
| 2 | `PropertyBasedTestingDemo` | Gerar entradas automaticamente, expressar invariantes e usar shrinking para encontrar o menor caso que viola uma regra. |
| 3 | `WebApiIntegrationTestingDemo` | Testar uma API em memória com `WebApplicationFactory`, substituir dependências e validar o contrato HTTP de ponta a ponta. |
| 4 | `MutationTestingDemo` | Usar mutation testing para avaliar a força da suíte e identificar testes que executam código sem verificar seu comportamento. |

## 13-SDKsAndLibraries

| # | Projeto sugerido | Finalidade didática |
|---:|---|---|
| 5 | `ResilientHttpSdk` | Projetar um SDK HTTP tipado com `HttpClientFactory`, configuração por options, cancelamento, retries e tradução consistente de erros. |
| 6 | `NuGetPackageLifecycleDemo` | Ensinar metadados de pacote, SemVer, `dotnet pack`, símbolos, documentação XML, feed local e consumo por outro projeto. |
| 7 | `IncrementalSourceGeneratorDemo` | Criar um source generator incremental com Roslyn, inspecionar o código gerado e testar entradas, diagnósticos e saídas. |

## Critérios usados na seleção

- Complementar os projetos existentes em vez de recriar seus exemplos centrais.
- Manter cada sample focado em um conceito principal e executável de forma independente.
- Misturar fundamentos, práticas de produção e tópicos avançados em uma progressão de aprendizado.
- Priorizar lacunas nas trilhas com menor quantidade de projetos, especialmente Testing e SDKsAndLibraries.
