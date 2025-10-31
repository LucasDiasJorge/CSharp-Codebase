# Por que usar (e não usar) o FusionCache em comparação a outros serviços de cache

Este documento resume os principais motivos para adotar ou evitar o uso do [FusionCache](https://github.com/ZiggyCreatures/FusionCache) em aplicações .NET, comparando-o rapidamente com alternativas comuns (IMemoryCache, Redis puro, LazyCache, EasyCaching, etc.).

## O que é o FusionCache
Uma biblioteca de cache de alto nível para .NET que combina:
- Prevenção de cache stampede (single-flight).
- Fail-safe (entrega de valores “stale” quando a fonte está indisponível).
- Atualização em segundo plano (background refresh).
- Multicamadas (memória + distribuído) com backplane.
- API simples (`GetOrSet`/`GetOrSetAsync`) e opções avançadas (timeouts, jitter, etc.).

No projeto, veja `Caching/FusionCache/Program.cs` para exemplos de:
- `GetOrSet`/`GetOrSetAsync` com options builder.
- `TryGet<T>` retornando `MaybeValue<T>`.
- Fail-safe + timeouts de fábrica.

## Principais benefícios (por que usar)
- Previne thundering herd/cache stampede: garante apenas uma execução da factory por chave, reduzindo carga em bases/APIs.
- Fail-safe integrado: em falhas temporárias, pode servir valor antigo (stale) para preservar a experiência do usuário.
- Atualização proativa: renova o item em background antes de expirar, reduzindo latência em acessos subsequentes.
- Multicamadas fácil: combina cache em memória com um distribuído (ex.: Redis via `IDistributedCache`) e usa backplane para invalidação entre instâncias.
- API produtiva: `GetOrSet` centraliza busca + cache + políticas (duração, timeouts), reduzindo boilerplate e erros.
- Opções de resiliência: timeouts de factory, cancelamento, jitter e políticas por item, sem precisar compor manualmente com Polly.

## Possíveis desvantagens (quando não usar)
- Dependência adicional: se você só precisa de um cache extremamente simples e local, `IMemoryCache` pode ser suficiente.
- Curva de aprendizado e configuração: recursos como fail-safe, background refresh e multicamadas exigem entendimento e parametrização correta.
- Overhead: há custo para coordenação/locks e telemetria; para cenários “micro” pode ser desnecessário.
- Controle de baixo nível: se você precisa de recursos específicos do Redis (scripts Lua, streams, pub/sub personalizado, structures avançadas), usar o cliente Redis diretamente pode ser melhor.
- Compatibilidade de API entre versões: atualizações podem introduzir mudanças de assinatura (ex.: factories `(ctx, ct)` e `TryGet` retornando `MaybeValue<T>`).

## Comparação rápida com alternativas
- IMemoryCache (Microsoft.Extensions.Caching.Memory)
  - Prós: built-in, leve, simples.
  - Contras: não traz nativamente anti-stampede, fail-safe ou background refresh; você precisa compor manualmente padrões de resiliência.
- Redis puro / IDistributedCache
  - Prós: compartilhado entre instâncias, TTL distribuído, escalável.
  - Contras: sem anti-stampede/fail-safe por padrão; maior latência; precisa implementar políticas e coordenação.
- LazyCache
  - Prós: API simples baseda em `IMemoryCache`, com lazy initialization que ajuda a reduzir duplicidade de factory.
  - Contras: recursos de resiliência e multicamadas são mais limitados comparados ao FusionCache.
- EasyCaching / CacheManager (com múltiplos provedores)
  - Prós: abstrações sobre vários backends, integração com Redis/Memcached/etc.
  - Contras: geralmente não oferecem o mesmo pacote integrado de anti-stampede + fail-safe + background refresh do FusionCache.

## Quando o FusionCache é uma ótima escolha
- Alto tráfego sobre dados caros de obter (APIs externas, queries complexas) e risco de cache stampede.
- Precisa de experiência resiliente: servir dados stale durante falhas temporárias e retentar em background.
- Aplicações distribuídas que precisam de cache em memória + distribuído com invalidação coordenada.
- Quer centralizar políticas de resiliência/timeout em um ponto (a própria chamada ao cache).

## Quando considerar outras opções
- Cenários simples e locais, onde `IMemoryCache` atende 100% sem requisitos de resiliência.
- Requisitos altamente específicos de Redis (streaming, estruturas customizadas, scripts Lua e afinados de latência).
- Quando o custo operacional de mais uma dependência não compensa os benefícios.

## Boas práticas com FusionCache
- Defina TTLs e jitter realistas por cenário; não use durações excessivas sem necessidade.
- Use factories assíncronas sempre que possível; ajuste `SetFactoryTimeouts`.
- Ative `SetFailSafe(true)` onde a experiência do usuário é crítica e a fonte pode falhar.
- Evite payloads muito grandes; escolha uma boa estratégia de serialização para a camada distribuída.
- Em múltiplas instâncias, configure backplane para propagar invalidações.
- Monitore métricas (hit/miss, tempo de factory, eventos de fail-safe) para calibrar políticas.

## Como isso se reflete no código deste projeto
O arquivo `Program.cs` demonstra:
- `GetOrSet`/`GetOrSetAsync` com options builder (`options.SetDuration(...)`).
- `TryGet<T>` usando `MaybeValue<T>` (`HasValue/Value`).
- Fail-safe com `SetFailSafe(true)` e timeouts de factory.