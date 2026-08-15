# CLAUDE.md — RedisCacheKeyParams

Console sobre composição de chaves de cache com `params object[]`. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/RedisCacheKeyParams/RedisCacheKeyParams.csproj
dotnet run --project 06-Caching/Caching/RedisCacheKeyParams/RedisCacheKeyParams.csproj
```

## Estrutura interna

- `Contracts/IRedisCacheKeys.cs` — contrato das chaves.
- `CacheKeys/RedisCacheKeys.cs` / `DefaultRedisCacheKeys.cs` — implementação com um único `Join(params object[] parts)` que aceita qualquer aridade.

O problema atacado: sem `params`, cada combinação de partes vira uma sobrecarga nova e a classe cresce sem limite. Com um método variádico, a explosão combinatória desaparece.

## Pontos de atenção

- TFM `net9.0`, **sem pacotes externos** — apesar do nome, não há `StackExchange.Redis` nem conexão com Redis. É só a estratégia de composição de chave, executável offline.
