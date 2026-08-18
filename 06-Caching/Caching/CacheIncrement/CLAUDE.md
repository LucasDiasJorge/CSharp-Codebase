# CLAUDE.md — CacheIncrement

API de contadores de alta performance: Redis na frente, MySQL como persistência. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
cd 06-Caching/Caching/CacheIncrement && docker compose up -d

dotnet run --project 06-Caching/Caching/CacheIncrement/CacheIncrement.csproj
```

Requisições de exemplo em `CacheIncrement.http`.

## Estrutura interna

Arquitetura em duas velocidades:

- `Services/CounterService.cs` — incrementa no Redis com `INCR`, **atômico e sub-milissegundo**. O banco não é tocado no caminho quente.
- `Services/CounterSyncService.cs` — background service que periodicamente descarrega os contadores do Redis para o MySQL. É aqui que está o trade-off do exemplo: throughput altíssimo em troca de uma janela de perda entre sincronizações.
- `Data/ApplicationDbContext.cs` — EF Core com Pomelo/MySQL.

## Pontos de atenção

- **BUILD QUEBRADO.** Sem `TargetFramework` no `.csproj` (`Directory.Build.props` removido no commit `50763d5`); falha com `NETSDK1013`. Pacotes na linha **8.0.x** → `net8.0` é o alvo coerente.
- **Único projeto da trilha com `docker-compose.yml` próprio** — ele sobe Redis e MySQL juntos. Use-o em vez de containers avulsos.
- Não "simplifique" gravando direto no MySQL a cada incremento: isso elimina exatamente o padrão demonstrado.
