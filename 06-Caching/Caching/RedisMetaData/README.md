# RedisMetaData

Pequeno exemplo didático do uso de Redis (StackExchange.Redis) para armazenar entradas em uma hash com metadado de expiração (`ExpiresAt`). Destinado a demonstrar leitura/gravação simples e padrões básicos de TTL via metadados.

## Pré-requisitos

- .NET 10 SDK
- Redis disponível (por padrão usamos `localhost:6379`). Exemplo com Docker:

```bash
docker run -d --name redis -p 6379:6379 redis:alpine
```

## Build e execução

Executando a partir da raiz do repositório (`06-Caching`):

```powershell
dotnet run --project Caching/RedisMetaData/RedisMetaData.csproj
```

Ou, a partir da pasta do projeto:

```powershell
cd Caching/RedisMetaData
dotnet restore
dotnet build
dotnet run
```

## Estrutura principal

- `Program.cs` — bootstrap e exemplo de uso.
- `Models/RedisConfig.cs` — configuração de host/porta/database.
- `Models/RedisEntry.cs` — wrapper das entradas com `Data` e `ExpiresAt`.
- `Services/RedisCache.cs` — implementação do cache (Set/Get) usando hashes.
- `RedisMetaData.csproj` — alvo `net10.0` e dependências (StackExchange.Redis).

## Como funciona (resumo)

- As entradas são salvas em uma hash com chave `main-key:{redisKey}`.
- Cada campo da hash armazena um JSON de `RedisEntry<TModel>` contendo `Data` e `ExpiresAt`.
- Na leitura a expiração é validada; se expirado, o campo é removido e `GetEntry` retorna `default`.
- Serialização com `System.Text.Json`.
- A conexão usa `ConnectionMultiplexer` e é fechada ao final (`Dispose`).

## Exemplo de uso

```csharp
using RedisMetaData.Models;
using RedisMetaData.Services;

var config = new RedisConfig { Host = "localhost", Port = 6379, Database = 0 };
using var cache = new RedisCache(config);

cache.SetEntry("user:1", "name", "Alice", TimeSpan.FromMinutes(10));
var name = cache.GetEntry<string>("user:1", "name");
Console.WriteLine(name);
```

## Observações e melhorias possíveis

- O TTL aqui é tratado como metadado e validado na leitura — para expiração automática use chaves separadas com `KeyExpire`.
- Em apps maiores, reutilize `ConnectionMultiplexer` como singleton via DI.
- Para testes, extraia `IRedisCache` e injete uma implementação mock.
- Considere um job de limpeza para campos expirados em hashes grandes.

## Dependências

- StackExchange.Redis (definido em `RedisMetaData.csproj`).

## Contribuições

Pull requests são bem-vindos — abra uma proposta com a melhoria desejada.

---
Arquivo atualizado para padronizar instruções e contexto do projeto.
