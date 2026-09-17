# CLAUDE.md — CacheStampedeProtectionDemo

API que reproduz cache stampede e o corrige com single-flight, jitter de TTL e stale-while-revalidate. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/CacheStampedeProtectionDemo/CacheStampedeProtectionDemo.csproj
dotnet run --project 06-Caching/Caching/CacheStampedeProtectionDemo/CacheStampedeProtectionDemo.csproj
```

**Exige Redis** em `localhost:6379`:

```bash
docker run -d --name cbk-redis -p 6379:6379 redis:7-alpine
```

Sobe em `http://localhost:5001`. O experimento comparativo está no README.

## Estrutura interna

`Caching/CacheStrategies` tem as três estratégias sobre o mesmo Redis e a mesma origem. `Origin/SlowDataSource` conta as chamadas — **é esse contador, não o tempo, que mede a proteção**.

**A linha que decide o exemplo** está em `GetSingleFlightAsync`: a *segunda* leitura do cache depois de `gate.WaitAsync`. Sem ela o semáforo não impede nada — apenas enfileira as 30 chamadas à origem, uma por vez. Resultado: mais lento que a versão ingênua e igualmente destrutivo. Se alguém "simplificar" removendo essa releitura, o número medido passa de 1 para 30 sem nenhum erro aparecer.

Em `GetStaleWhileRevalidateAsync`, `_refreshing.TryAdd` garante **uma** revalidação por chave. Sem esse controle, cada requisição que encontra valor velho dispara a sua e o stampede volta pela porta dos fundos.

`TtlJitter.Apply` sorteia por chamada, não por processo — duas chaves gravadas no mesmo milissegundo precisam receber prazos diferentes.

## Pontos de atenção

- TFM `net10.0`. Pacote: `StackExchange.Redis` 3.2.1 (a trilha usa 2.7–2.12 nos projetos antigos; aqui é a versão atual).
- **Serviço externo obrigatório:** Redis. Sem ele, `ConnectionMultiplexer.Connect` falha no start.
- **Limitação declarada, não defeito:** o single-flight usa `SemaphoreSlim` em memória e coordena apenas **uma instância**. Com várias réplicas, cada uma faz sua chamada à origem. Está comentado no código e no README, com ponteiro para [RedisDistributedLockDemo](../RedisDistributedLockDemo/CLAUDE.md). Não "consertar" sem pedido — a versão distribuída é outro projeto.
- `_keyGates` cresce indefinidamente com chaves ilimitadas. Citado no README como cuidado de produção; aceitável no exemplo.
- Números do README medidos com origem de 400ms e 30 requisições: naive = 30 chamadas, single-flight = 1. Alterar `SlowDataSource.Latency` ou a concorrência muda esses valores no texto.
- Os TTLs (`BaseTtl` 10s, `SoftTtl` 3s) aparecem no README em dois lugares — o ciclo do SWR e a tabela de jitter. Alterar exige atualizar ambos.
- O cenário "2 valores distintos" no experimento naive é **esperado**: várias respostas da origem sobrescrevem a chave. Não é bug; está explicado no README.
- **Fronteira com os vizinhos**: `CacheAside` e `CachePatterns` cobrem os padrões de leitura/escrita. Aqui o assunto é exclusivamente o que acontece na expiração sob concorrência.
