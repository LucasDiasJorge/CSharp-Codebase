# Por que usar (e n�o usar) o FusionCache em compara��o a outros servi�os de cache

Este documento resume os principais motivos para adotar ou evitar o uso do [FusionCache](https://github.com/ZiggyCreatures/FusionCache) em aplica��es .NET, comparando-o rapidamente com alternativas comuns (IMemoryCache, Redis puro, LazyCache, EasyCaching, etc.).

## O que � o FusionCache
Uma biblioteca de cache de alto n�vel para .NET que combina:
- Preven��o de cache stampede (single-flight).
- Fail-safe (entrega de valores �stale� quando a fonte est� indispon�vel).
- Atualiza��o em segundo plano (background refresh).
- Multicamadas (mem�ria + distribu�do) com backplane.
- API simples (`GetOrSet`/`GetOrSetAsync`) e op��es avan�adas (timeouts, jitter, etc.).

No projeto, veja `Caching/FusionCache/Program.cs` para exemplos de:
- `GetOrSet`/`GetOrSetAsync` com options builder.
- `TryGet<T>` retornando `MaybeValue<T>`.
- Fail-safe + timeouts de f�brica.

## Principais benef�cios (por que usar)
- Previne thundering herd/cache stampede: garante apenas uma execu��o da factory por chave, reduzindo carga em bases/APIs.
- Fail-safe integrado: em falhas tempor�rias, pode servir valor antigo (stale) para preservar a experi�ncia do usu�rio.
- Atualiza��o proativa: renova o item em background antes de expirar, reduzindo lat�ncia em acessos subsequentes.
- Multicamadas f�cil: combina cache em mem�ria com um distribu�do (ex.: Redis via `IDistributedCache`) e usa backplane para invalida��o entre inst�ncias.
- API produtiva: `GetOrSet` centraliza busca + cache + pol�ticas (dura��o, timeouts), reduzindo boilerplate e erros.
- Op��es de resili�ncia: timeouts de factory, cancelamento, jitter e pol�ticas por item, sem precisar compor manualmente com Polly.

## Poss�veis desvantagens (quando n�o usar)
- Depend�ncia adicional: se voc� s� precisa de um cache extremamente simples e local, `IMemoryCache` pode ser suficiente.
- Curva de aprendizado e configura��o: recursos como fail-safe, background refresh e multicamadas exigem entendimento e parametriza��o correta.
- Overhead: h� custo para coordena��o/locks e telemetria; para cen�rios �micro� pode ser desnecess�rio.
- Controle de baixo n�vel: se voc� precisa de recursos espec�ficos do Redis (scripts Lua, streams, pub/sub personalizado, structures avan�adas), usar o cliente Redis diretamente pode ser melhor.
- Compatibilidade de API entre vers�es: atualiza��es podem introduzir mudan�as de assinatura (ex.: factories `(ctx, ct)` e `TryGet` retornando `MaybeValue<T>`).

## Compara��o r�pida com alternativas
- IMemoryCache (Microsoft.Extensions.Caching.Memory)
  - Pr�s: built-in, leve, simples.
  - Contras: n�o traz nativamente anti-stampede, fail-safe ou background refresh; voc� precisa compor manualmente padr�es de resili�ncia.
- Redis puro / IDistributedCache
  - Pr�s: compartilhado entre inst�ncias, TTL distribu�do, escal�vel.
  - Contras: sem anti-stampede/fail-safe por padr�o; maior lat�ncia; precisa implementar pol�ticas e coordena��o.
- LazyCache
  - Pr�s: API simples baseda em `IMemoryCache`, com lazy initialization que ajuda a reduzir duplicidade de factory.
  - Contras: recursos de resili�ncia e multicamadas s�o mais limitados comparados ao FusionCache.
- EasyCaching / CacheManager (com m�ltiplos provedores)
  - Pr�s: abstra��es sobre v�rios backends, integra��o com Redis/Memcached/etc.
  - Contras: geralmente n�o oferecem o mesmo pacote integrado de anti-stampede + fail-safe + background refresh do FusionCache.

## Quando o FusionCache � uma �tima escolha
- Alto tr�fego sobre dados caros de obter (APIs externas, queries complexas) e risco de cache stampede.
- Precisa de experi�ncia resiliente: servir dados stale durante falhas tempor�rias e retentar em background.
- Aplica��es distribu�das que precisam de cache em mem�ria + distribu�do com invalida��o coordenada.
- Quer centralizar pol�ticas de resili�ncia/timeout em um ponto (a pr�pria chamada ao cache).

## Quando considerar outras op��es
- Cen�rios simples e locais, onde `IMemoryCache` atende 100% sem requisitos de resili�ncia.
- Requisitos altamente espec�ficos de Redis (streaming, estruturas customizadas, scripts Lua e afinados de lat�ncia).
- Quando o custo operacional de mais uma depend�ncia n�o compensa os benef�cios.

## Boas pr�ticas com FusionCache
- Defina TTLs e jitter realistas por cen�rio; n�o use dura��es excessivas sem necessidade.
- Use factories ass�ncronas sempre que poss�vel; ajuste `SetFactoryTimeouts`.
- Ative `SetFailSafe(true)` onde a experi�ncia do usu�rio � cr�tica e a fonte pode falhar.
- Evite payloads muito grandes; escolha uma boa estrat�gia de serializa��o para a camada distribu�da.
- Em m�ltiplas inst�ncias, configure backplane para propagar invalida��es.
- Monitore m�tricas (hit/miss, tempo de factory, eventos de fail-safe) para calibrar pol�ticas.

## Como isso se reflete no c�digo deste projeto
O arquivo `Program.cs` demonstra:
- `GetOrSet`/`GetOrSetAsync` com options builder (`options.SetDuration(...)`).
- `TryGet<T>` usando `MaybeValue<T>` (`HasValue/Value`).
- Fail-safe com `SetFailSafe(true)` e timeouts de factory.