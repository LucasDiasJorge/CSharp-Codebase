# CLAUDE.md — RedisHashFieldExpire

Console focado exclusivamente em **TTL por campo de hash** no Redis. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name redis -p 6379:6379 redis

dotnet run --project 06-Caching/Caching/RedisHashFieldExpire/RedisHashFieldExpire.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`), escopo deliberadamente mínimo: cria o hash, aplica `HashFieldExpire` a campos específicos e imprime o retorno da operação.

O ponto: historicamente o TTL do Redis é **por chave**; expiração por campo dentro de um hash é recurso recente (Redis 7.4+). Por isso o exemplo é isolado — o retorno da chamada é o que confirma se o servidor suporta.

## Pontos de atenção

- **Exige Redis 7.4 ou superior**. Em servidor mais antigo o comando não existe e a operação falha ou retorna erro — isso não é bug do código.
- TFM `net9.0`. `StackExchange.Redis` 2.8.37.
- Compare com `RedisMetaData`, que resolve necessidade parecida **sem** o recurso nativo, guardando `ExpiresAt` como metadado.
