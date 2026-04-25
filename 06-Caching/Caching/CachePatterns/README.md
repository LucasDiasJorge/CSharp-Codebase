# CachePatterns (Comparativo de Estratégias de Cache)

## Visão geral

Projeto console (.NET 9 via props global) demonstrando 8 padrões de cache: Cache-Aside, Write-Through, Write-Behind, Read-Through, Refresh-Ahead, Full Cache, Near Cache e Tiered (multi-level).

## Conceitos abordados

- Exemplo didático sobre CachePatterns (Comparativo de Estratégias de Cache) no contexto de estratégias de cache e integração com Redis.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

Fornecer visão prática e comparativa dos trade-offs entre latência, consistência, consumo de memória e complexidade em estratégias de caching.

## Estrutura do projeto

```text
CachePatterns/
+-- .vscode/
|   \-- tasks.json
+-- Data/
|   \-- Repository.cs
+-- Models/
|   \-- Models.cs
+-- Patterns/
|   +-- CacheAsidePattern.cs
|   +-- FullCachePattern.cs
|   +-- NearCachePattern.cs
|   +-- ReadThroughPattern.cs
|   +-- RefreshAheadPattern.cs
|   +-- TieredCachePattern.cs
|   +-- WriteBehindPattern.cs
|   \-- WriteThroughPattern.cs
+-- appsettings.json
+-- CachePatterns.csproj
+-- CachePatterns.sln
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 06-Caching/Caching/CachePatterns/CachePatterns.csproj
```

Console exibirá seções com hits/misses e comportamento de cada padrão.

## Boas práticas e pontos de atenção

- Chaves previsíveis e niveladas.
- Promoção entre níveis (Tiered/Near).
- Separação repositório vs política de cache.
- Observabilidade de hit ratio.

## Conteúdo complementar

##### 2. Padrões (Resumo)

| Padrão | Uso Ideal | Risco / Trade-off |
|--------|-----------|-------------------|
| Cache-Aside | Dados consultados esporadicamente | Miss inicial / lógica duplicada |
| Write-Through | Consistência forte | Escritas mais lentas |
| Write-Behind | Alto volume de escrita | Perda em falha antes de flush |
| Read-Through | Abstração total | Menos controle granular |
| Refresh-Ahead | Dados críticos frequentes | Refresh redundante |
| Full Cache | Referências estáveis | Bootstrap lento / memória |
| Near Cache | Distribuído baixa latência | Coerência difícil |
| Tiered | Alta escala / heterogêneo | Complexidade operacional |

##### 3. Estrutura

```
CachePatterns/
    Patterns/ (implementações)
    Data/ (repositório simulado)
    Models/ (entidades simples)
    Program.cs (execução sequencial)
```

##### 5. Métricas (Exemplo)

```csharp
Metrics metrics = tieredCacheService.GetMetrics();
Console.WriteLine(metrics.CacheHitRate);
```
Para evolução: exportar para Prometheus / OpenTelemetry.

##### 6. Configuráveis

- TTL por padrão ajustável.
- Política de eviction via `MemoryCacheEntryOptions`.
- Limites de memória e compaction configuráveis em `AddMemoryCache`.

##### 8. Produção (Checklist Resumido)

- Migrar para Redis distribuído (L2) / memory local (L1).
- Implementar circuit breaker para backend.
- Adicionar tracing de latência por camada.
- Evitar full cache em datasets voláteis.

##### 9. Extensões Futuras

- Cache warming seletivo.
- Métricas de p95/p99 latência.
- Adaptação para distributed lock em Write-Behind.

## Referências

Microsoft Docs (Caching), Azure Architecture Patterns, Redis Patterns, Fowler Distributed Systems.
