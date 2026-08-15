# CLAUDE.md — RedisMetaData

Console que guarda entradas em hash Redis com **metadado de expiração** (`ExpiresAt`) controlado pela aplicação. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name redis -p 6379:6379 redis

dotnet run --project 06-Caching/Caching/RedisMetaData/RedisMetaData.csproj
```

## Estrutura interna

- `Models/RedisConfig.cs` — configuração de conexão.
- `Models/RedisEntry.cs` — o valor **mais** seu `ExpiresAt`.
- `Services/RedisCache.cs` — leitura/gravação; a validade é conferida no código, comparando `ExpiresAt` com o instante atual.

Estratégia oposta à de `RedisHashFieldExpire`: aqui o Redis não expira nada, a aplicação decide. Funciona em qualquer versão do servidor e permite saber *que* a entrada expirou (o dado continua lá), ao custo de limpeza manual.

## Pontos de atenção

- TFM **`net10.0`** (o README exige .NET 10 SDK). `StackExchange.Redis` 2.12.14.
- **Exige Redis ativo** em `localhost:6379`.
- Entradas expiradas **não são removidas** pelo servidor; sem rotina de limpeza, a memória cresce indefinidamente. É a contrapartida da abordagem.
