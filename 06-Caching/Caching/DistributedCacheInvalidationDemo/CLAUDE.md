# CLAUDE.md — DistributedCacheInvalidationDemo

API com três nós simulados que sincronizam a invalidação de cache local por Redis Pub/Sub. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/DistributedCacheInvalidationDemo/DistributedCacheInvalidationDemo.csproj
dotnet run --project 06-Caching/Caching/DistributedCacheInvalidationDemo/DistributedCacheInvalidationDemo.csproj
```

**Exige Redis** em `localhost:6379`:

```bash
docker run -d --name cbk-redis -p 6379:6379 redis:7-alpine
```

Sobe em `http://localhost:5090` com os nós `A`, `B` e `C`. Roteiro em `DistributedCacheInvalidationDemo.http`.

## Estrutura interna

`Cache/CacheNode` é um nó: `ConcurrentDictionary` próprio como L1, Redis compartilhado como L2, assinatura própria em `cache:invalidation`. `Cache/NodeCluster` cria três deles.

**Os três nós vivem no mesmo processo de propósito**, para o sample rodar com um comando. Cada um tem L1 independente e assinatura independente — a mesma situação de três réplicas. O Pub/Sub passa pelo Redis de verdade, não há atalho em memória.

`WriteAsync` tem uma ordem que não pode mudar: grava na origem → apaga do Redis → remove do L1 → **publica**. Publicar antes de gravar abriria janela para outro nó recarregar o valor antigo e recolocá-lo no cache.

**Publica-se a invalidação, nunca o valor novo.** Com duas escritas em sequência e mensagens fora de ordem, publicar valor grava o dado errado em todos os nós. Está comentado no código; não "otimizar" mandando o valor junto.

`HandleInvalidation` ignora a mensagem do próprio nó (contabilizada em `InvalidationsIgnored`) — o nó já removeu a entrada antes de publicar.

## Pontos de atenção

- TFM `net10.0`. Pacote: `StackExchange.Redis` 3.2.1 — nesta versão o canal é tipado: `RedisChannel.Literal("...")`, não uma string solta.
- **Serviço externo obrigatório:** Redis. Sem ele, `ConnectionMultiplexer.Connect` falha no start.
- O `PUT` com invalidação tem um `Task.Delay(100)` antes de responder. **É andaime do exemplo**, para que a leitura seguinte já encontre o estado convergido; não é parte do padrão. Está comentado no código — não replicar em produção, e não remover sem ajustar o roteiro do README, que depende de a leitura seguinte já ver o valor novo.
- O endpoint `/no-invalidation` existe para mostrar o defeito. **Não remover** — é metade do exemplo.
- TTL de 5 minutos no L2 é a rede de segurança para aviso perdido. O README explica por que ele continua necessário mesmo com invalidação por evento.
- O L1 **não tem TTL próprio**: só sai por invalidação ou reset. É simplificação deliberada, para a divergência ser permanente e observável; em produção o L1 também teria expiração.
- Os valores citados no README (`R$ 349,90` → `R$ 299,00` / `R$ 279,00`) vêm do seed de `ProductRepository` e dos roteiros. Alterar o seed invalida as tabelas.
- **Fronteira com os vizinhos**: `CacheAside` e `CachePatterns` cobrem leitura/escrita numa instância; [CacheStampedeProtectionDemo](../CacheStampedeProtectionDemo/CLAUDE.md) cobre expiração sob concorrência. Aqui o assunto é exclusivamente a coerência entre instâncias.
