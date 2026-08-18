# CLAUDE.md — RedisConsoleApp

Console com as operações essenciais do Redis via `StackExchange.Redis`. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name redis -p 6379:6379 redis

dotnet run --project 06-Caching/Caching/RedisConsoleApp/RedisConsoleApp.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): strings, TTL, hashes, listas e sets, cada tipo em seu bloco. É o ponto de partida da trilha para quem nunca usou o cliente.

## Pontos de atenção

- **Exige Redis ativo** em `localhost:6379`; sem ele a execução falha na conexão.
- TFM `net9.0`. `StackExchange.Redis` 2.8.37.
- `ConnectionMultiplexer` é caro e **thread-safe**: deve ser criado uma vez e reutilizado. Não o instancie por operação ao estender o exemplo.
