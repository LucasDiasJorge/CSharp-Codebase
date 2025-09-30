git clone <repository-url>
dotnet restore
dotnet run
<!-- README padronizado (versão condensada) -->
# CachePatterns (Comparativo de Estratégias de Cache)

Projeto console (.NET 9 via props global) demonstrando 8 padrões de cache: Cache-Aside, Write-Through, Write-Behind, Read-Through, Refresh-Ahead, Full Cache, Near Cache e Tiered (multi-level).

## 1. Objetivo
Fornecer visão prática e comparativa dos trade-offs entre latência, consistência, consumo de memória e complexidade em estratégias de caching.

## 2. Padrões (Resumo)
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

## 3. Estrutura
```
CachePatterns/
    Patterns/ (implementações)
    Data/ (repositório simulado)
    Models/ (entidades simples)
    Program.cs (execução sequencial)
```

## 4. Execução
```powershell
cd "C:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\CachePatterns"
dotnet run
```

Console exibirá seções com hits/misses e comportamento de cada padrão.

## 5. Métricas (Exemplo)
```csharp
Metrics metrics = tieredCacheService.GetMetrics();
Console.WriteLine(metrics.CacheHitRate);
```
Para evolução: exportar para Prometheus / OpenTelemetry.

## 6. Configuráveis
- TTL por padrão ajustável.
- Política de eviction via `MemoryCacheEntryOptions`.
- Limites de memória e compaction configuráveis em `AddMemoryCache`.

## 7. Boas Práticas Demonstradas
- Chaves previsíveis e niveladas.
- Promoção entre níveis (Tiered/Near).
- Separação repositório vs política de cache.
- Observabilidade de hit ratio.

## 8. Produção (Checklist Resumido)
- Migrar para Redis distribuído (L2) / memory local (L1).
- Implementar circuit breaker para backend.
- Adicionar tracing de latência por camada.
- Evitar full cache em datasets voláteis.

## 9. Extensões Futuras
- Cache warming seletivo.
- Métricas de p95/p99 latência.
- Adaptação para distributed lock em Write-Behind.

## 10. Referências
Microsoft Docs (Caching), Azure Architecture Patterns, Redis Patterns, Fowler Distributed Systems.

---
Versão condensada – conteúdo original extenso sintetizado.
