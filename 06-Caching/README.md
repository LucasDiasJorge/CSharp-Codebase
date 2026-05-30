# 06-Caching

## Visão geral

Trilha dedicada a estratégias de cache em .NET (ex.: .NET 10). O repositório contém exemplos práticos que cobrem cache em memória, cache distribuído com Redis, integração com banco relacional, resiliência contra falhas e uma pequena SDK para unificar provedores.

Esta pasta está organizada em dois blocos principais: a coleção [Caching](./Caching/README.md), que reúne os exemplos executáveis de estudo, e o [UnifiedCacheSdk](./UnifiedCacheSdk/README.md), que demonstra uma abordagem reutilizável para uniformizar o acesso a provedores de cache.

## Conceitos abordados

- `IMemoryCache`, Redis e cache distribuído.
- Cache-Aside, contadores atômicos, comparativo de padrões e cache híbrido/resiliente.
- TTL, invalidação seletiva, hit/miss ratio e fallback em caso de falha do cache.
- Critérios para decidir quando cachear, quando expirar rápido e quando não cachear.
- Padronização de chaves, isolamento por serviço e abstração de provedor.

## Objetivos de aprendizagem

- Entender quando usar cada estratégia de cache de acordo com o cenário.
- Executar exemplos isolados com comandos direcionados ao `.csproj` correto.
- Comparar soluções simples, distribuídas e resilientes dentro da mesma trilha.
- Diferenciar dados estáveis, semi-estáveis e transitórios antes de introduzir cache.
- Reaproveitar a documentação local como mapa de estudo e referência prática.

## Estrutura do repositório

```text
06-Caching/
+-- Caching/
|   +-- README.md
|   +-- CacheAside/
|   +-- CacheIncrement/
|   +-- CachePatterns/
|   +-- FusionCache/
|   +-- RedisConsoleApp/
|   +-- RedisCacheKeyParams/
|   +-- RedisHashFieldExpire/
|   \-- RedisMySQLIntegration/
+-- UnifiedCacheSdk/
|   +-- README.md
|   \-- src/
\-- README.md
```

## Como executar

Executar a partir da raiz do repositório (`06-Caching`). Exemplos principais:

```bash
dotnet run --project Caching/CacheAside/CacheAside.csproj
dotnet run --project Caching/CacheIncrement/CacheIncrement.csproj
dotnet run --project Caching/CachePatterns/CachePatterns.csproj
dotnet run --project Caching/FusionCache/FusionCache.csproj
dotnet run --project Caching/RedisConsoleApp/RedisConsoleApp.csproj
dotnet run --project Caching/RedisCacheKeyParams/RedisCacheKeyParams.csproj
dotnet run --project Caching/RedisHashFieldExpire/RedisHashFieldExpire.csproj
dotnet run --project Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj
```

SDK reutilizável:

```bash
dotnet build UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj
```

> Observação: alguns exemplos exigem serviços externos (Redis, MySQL). Use Docker para facilitar a execução local.

## Boas práticas e pontos de atenção

- Use chaves previsíveis e estruturadas, como `produto:123` ou `pedido:tenant-a:987`.
- Defina TTL de acordo com a volatilidade do dado, evitando expiração longa para dados muito dinâmicos.
- Prefira invalidação seletiva em vez de limpar o cache inteiro sem necessidade.
- Mantenha fallback para banco ou fonte original quando o cache falhar ou estiver indisponível.
- Monitore latência, hit/miss ratio e custo de serialização antes de ampliar o uso de cache.
- Valide dependências externas antes de executar (Redis, MySQL quando aplicável).

## Artigo: por que dados transitórios não devem ser cacheados?

Quando um senior diz que dados transitórios não devem ser cacheados, a mensagem central é simples: se o dado muda mais rápido do que a janela em que ele seria reutilizado com segurança, o cache deixa de ser otimização e passa a ser fonte de inconsistência.

Em outras palavras, cache só ajuda quando existe chance real de reaproveitamento do mesmo valor antes que ele perca validade.

### O que são dados transitórios?

Dados transitórios são informações com vida útil muito curta ou taxa de mudança muito alta. Exemplos comuns:

- status em tempo real, como `usuário está digitando`;
- posição de cursor, scroll ou presença online;
- estado intermediário de uma requisição ainda em processamento;
- tokens de uso único, como OTP ou CSRF;
- rascunho momentâneo de formulário;
- leituras operacionais que mudam a cada segundo e exigem precisão imediata.

O ponto não é apenas o dado mudar. O ponto é ele mudar tão rápido que, quando o sistema tentar reutilizá-lo, o valor já estará velho.

### Por que cachear esse tipo de dado costuma dar errado?

| Problema | O que acontece na prática |
|---------|---------------------------|
| Stale quase imediato | O valor expira semanticamente antes mesmo de o TTL acabar. |
| Custo maior que benefício | Você gasta memória, serialização e invalidação sem reduzir trabalho relevante. |
| Bugs difíceis de reproduzir | A aplicação passa a alternar entre dado novo e dado antigo de forma pouco previsível. |
| Invalidação excessiva | O time precisa invalidar cache quase o tempo todo, anulando a vantagem da camada. |
| Percepção errada do domínio | O sistema trata como leitura reaproveitável algo que, na prática, é estado momentâneo. |

### Regra prática para decidir

Antes de colocar um dado no cache, vale fazer quatro perguntas:

1. Esse valor será lido novamente antes de mudar?
2. O sistema tolera alguns segundos ou minutos de defasagem?
3. Existe uma estratégia simples de invalidação quando ocorrer escrita?
4. O custo de buscar na origem é alto o suficiente para justificar a camada extra?

Se a resposta for "não" para a maior parte dessas perguntas, provavelmente não é caso de cache.

### Exemplo concreto

Imagine o saldo de uma conta sendo cacheado por 5 minutos. Se uma transferência acontecer logo após a primeira leitura, o usuário continuará vendo um saldo antigo durante toda a janela de TTL. Nesse cenário, o cache não está acelerando uma leitura segura; ele está atrasando a verdade.

O mesmo raciocínio vale para presença online, progresso de upload, contagem de pessoas conectadas naquele segundo ou qualquer outro dado cuja utilidade depende de estar praticamente em tempo real.

### O que cachear no lugar?

Cache tende a funcionar bem para dados estáveis ou semi-estáveis, como:

- catálogos e listas de referência, como países, estados e categorias;
- configurações do sistema;
- resultados de consultas pesadas com baixa frequência de mudança;
- read models montados para consulta;
- páginas ou fragmentos com conteúdo pouco volátil.

### Como isso conversa com os exemplos desta trilha?

Na pasta [CacheAside](./Caching/CacheAside/README.md), o cache faz sentido porque produtos e listas de produtos têm chance real de reaproveitamento entre leituras e aceitam TTL curto com invalidação seletiva após escrita.

Já em [CacheIncrement](./Caching/CacheIncrement/README.md), o Redis não está atuando apenas como uma cópia temporária de leitura. Ele funciona como camada operacional rápida para incrementos atômicos, com sincronização posterior para MySQL. Essa distinção é importante: nem todo dado muito dinâmico deve virar cache de leitura, mas ele pode viver em uma camada rápida quando essa camada faz parte do desenho de consistência do sistema.

### Resumo prático

O conselho do senior significa: não desperdice cache com informações que envelhecem antes de gerar reutilização real. Cache deve reduzir custo de leitura sem comprometer entendimento do domínio. Quando a invalidação é constante, a precisão precisa ser imediata ou o valor só existe naquele instante, a melhor decisão costuma ser não cachear.

## Conteúdo complementar

### Mapa rápido da trilha

| Projeto | Foco principal | Dependências | Comando principal |
|---------|----------------|--------------|-------------------|
| [Caching](./Caching/README.md) | Visão geral da coleção de exemplos | Varia por subprojeto | Consulte o README da pasta |
| [CacheAside](./Caching/CacheAside/README.md) | Padrão Cache-Aside em API | Memória local | `dotnet run --project Caching/CacheAside/CacheAside.csproj` |
| [CacheIncrement](./Caching/CacheIncrement/README.md) | Contadores de alta performance | Redis + MySQL | `dotnet run --project Caching/CacheIncrement/CacheIncrement.csproj` |
| [CachePatterns](./Caching/CachePatterns/README.md) | Comparativo de estratégias | Console local | `dotnet run --project Caching/CachePatterns/CachePatterns.csproj` |
| [FusionCache](./Caching/FusionCache/README.md) | Resiliência e anti-stampede | Biblioteca FusionCache | `dotnet run --project Caching/FusionCache/FusionCache.csproj` |
| [RedisConsoleApp](./Caching/RedisConsoleApp/README.md) | Operações essenciais com Redis | Redis | `dotnet run --project Caching/RedisConsoleApp/RedisConsoleApp.csproj` |
| [RedisCacheKeyParams](./Caching/RedisCacheKeyParams/README.md) | Composição de chaves com params object[] | Console local | `dotnet run --project Caching/RedisCacheKeyParams/RedisCacheKeyParams.csproj` |
| [RedisHashFieldExpire](./Caching/RedisHashFieldExpire/README.md) | Expiração por campo em hash com HEXPIRE | Redis 7.4+ | `dotnet run --project Caching/RedisHashFieldExpire/RedisHashFieldExpire.csproj` |
| [RedisMySQLIntegration](./Caching/RedisMySQLIntegration/README.md) | Cache distribuído com persistência | Redis + MySQL | `dotnet run --project Caching/RedisMySQLIntegration/RedisMySQLIntegration.csproj` |
| [UnifiedCacheSdk](./UnifiedCacheSdk/README.md) | SDK para unificar acesso a cache | Memory ou Redis | `dotnet build UnifiedCacheSdk/src/UnifiedCacheSdk/UnifiedCacheSdk.csproj` |

### Ordem de estudo recomendada

1. [CacheAside](./Caching/CacheAside/README.md) → Fundamentos e padrão mais comum
2. [CachePatterns](./Caching/CachePatterns/README.md) → Visão comparativa de estratégias
3. [RedisConsoleApp](./Caching/RedisConsoleApp/README.md) → Recursos do Redis
4. [RedisCacheKeyParams](./Caching/RedisCacheKeyParams/README.md) → Chaves de cache com params object[]
5. [RedisHashFieldExpire](./Caching/RedisHashFieldExpire/README.md) → Expiração por campo em hashes
6. [RedisMySQLIntegration](./Caching/RedisMySQLIntegration/README.md) → Cache distribuído com persistência
7. [CacheIncrement](./Caching/CacheIncrement/README.md) → Escrita intensiva e sincronização
8. [FusionCache](./Caching/FusionCache/README.md) → Resiliência e recursos avançados
9. [UnifiedCacheSdk](./UnifiedCacheSdk/README.md) → Padronização e reuso em soluções maiores

## Referências e documentação complementar

- [Microsoft Docs - Overview of caching in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/overview)
- [Redis documentation](https://redis.io/docs/)
- [FusionCache GitHub](https://github.com/ZiggyCreatures/FusionCache)
- [Caching/README.md](./Caching/README.md)
- [UnifiedCacheSdk/README.md](./UnifiedCacheSdk/README.md)