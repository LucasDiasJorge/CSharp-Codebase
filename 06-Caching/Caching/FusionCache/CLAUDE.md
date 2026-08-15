# CLAUDE.md — FusionCache

Console demonstrando a biblioteca FusionCache e seus mecanismos de resiliência. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/FusionCache/FusionCache.csproj
dotnet run --project 06-Caching/Caching/FusionCache/FusionCache.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`) organizado em torno de `GetOrSet`/`GetOrSetAsync`. O valor do exemplo está nos recursos que essa única chamada embute e que os padrões manuais da trilha não têm:

- **Anti-stampede** — uma só execução da factory por chave sob concorrência (evita thundering herd).
- **Fail-safe** — devolve valor expirado quando a factory falha, em vez de propagar o erro.
- **Soft/hard timeout** — corta factory lenta sem derrubar a requisição.

## Pontos de atenção

- TFM **`net10.0`** (a maioria da trilha está em `net9.0`/`net8.0`). Preserve ao editar.
- Pacote: `ZiggyCreatures.FusionCache` 2.4.0. Roda **em memória**, sem Redis.
- É o contraponto "biblioteca pronta" aos padrões implementados à mão em `CachePatterns` — a comparação entre os dois é o ponto didático.
