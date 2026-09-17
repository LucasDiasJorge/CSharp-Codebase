# CacheStampedeProtectionDemo

API que reproduz um cache stampede e o corrige com single-flight, TTL com jitter e stale-while-revalidate, medindo o efeito de cada técnica sobre a fonte de dados.

## Visão geral

Cache stampede é o que acontece no instante em que uma chave popular expira. Todas as requisições que chegam nesse momento erram o cache ao mesmo tempo, e todas vão à origem — que justamente por ser lenta ainda não teve tempo de responder à primeira. Trinta requisições simultâneas viram trinta consultas ao banco, exatamente quando o cache deveria estar protegendo-o.

A medida que importa não é o tempo de resposta, e sim **quantas vezes a origem foi consultada**. Sem proteção, trinta requisições produzem trinta chamadas. Com single-flight, uma. O tempo total percebido pelo usuário é o mesmo nos dois casos — o que muda é a carga.

O single-flight funciona com um semáforo por chave: quem chega primeiro vai à origem, os demais esperam e, ao entrar, encontram o cache já preenchido. Há um detalhe que decide entre funcionar e piorar tudo: a segunda checagem do cache depois de adquirir o semáforo. Sem ela, o semáforo apenas enfileira as trinta chamadas em vez de paralelizá-las.

O jitter no TTL trata de um caso que passa despercebido. Se muitas chaves são populadas no mesmo instante — depois de um deploy, ou de um `FLUSHALL` — um TTL fixo faz todas expirarem juntas, e o stampede volta multiplicado por todas as chaves de uma vez.

O stale-while-revalidate ataca por outro lado: em vez de fazer alguém esperar pela atualização, serve o valor velho na hora e atualiza em segundo plano. Ninguém espera; o preço é servir dado desatualizado por alguns instantes.

## Conceitos abordados

- Cache stampede e a janela aberta pela latência da origem.
- Single-flight com semáforo por chave.
- Dupla checagem do cache após adquirir o semáforo.
- Limite do single-flight em processo: ele não coordena várias instâncias.
- TTL com jitter contra expiração sincronizada.
- Stale-while-revalidate com TTL curto (soft) e TTL duro.
- Revalidação única em segundo plano, sem bloquear ninguém.
- Falha na revalidação não derruba o valor antigo.

## Objetivos de aprendizagem

- Medir proteção de cache pela carga na origem, não pelo tempo de resposta.
- Implementar single-flight sem transformá-lo em fila de chamadas.
- Reconhecer quando o problema é a expiração sincronizada de muitas chaves.
- Decidir entre fazer alguém esperar e servir dado levemente velho.
- Saber o que o single-flight em processo não resolve.

## Estrutura do projeto

```text
CacheStampedeProtectionDemo/
|-- Caching/
|   |-- CacheStrategies.cs
|   `-- TtlJitter.cs
|-- Origin/
|   `-- SlowDataSource.cs
|-- Properties/
|   `-- launchSettings.json
|-- CacheStampedeProtectionDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

Requer Redis:

```bash
docker run -d --name cbk-redis -p 6379:6379 redis:7-alpine
```

Depois:

```bash
dotnet run --project 06-Caching/Caching/CacheStampedeProtectionDemo/CacheStampedeProtectionDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 06-Caching/Caching/CacheStampedeProtectionDemo/CacheStampedeProtectionDemo.csproj
```

A API sobe em `http://localhost:5001`.

O experimento que mostra a diferença:

```bash
curl -s -X POST http://localhost:5001/experiment -H "Content-Type: application/json" \
  -d '{"key":"produto:1","strategy":"naive","concurrency":30}'

curl -s -X POST http://localhost:5001/experiment -H "Content-Type: application/json" \
  -d '{"key":"produto:2","strategy":"singleflight","concurrency":30}'
```

## Boas práticas e pontos de atenção

- Meça a carga na origem, não a latência. As duas estratégias respondem em ~430ms; o que as separa é 30 consultas contra 1.
- Não esqueça a segunda checagem do cache depois de adquirir o semáforo. Sem ela, cada requisição em espera entra e vai à origem por conta própria — o resultado é serializado e igualmente destrutivo, só que mais lento.
- Single-flight com semáforo em memória coordena **uma instância**. Com dez réplicas, o pior caso volta a ser dez chamadas simultâneas à origem. Para coordenar entre processos é preciso lock distribuído — ver `RedisDistributedLockDemo`.
- Dê jitter ao TTL sempre que muitas chaves forem populadas juntas. O custo é zero e evita o stampede sincronizado que só aparece em produção, depois de um restart.
- No stale-while-revalidate, dispare **uma** revalidação por chave. Sem o controle de "já estou atualizando", cada requisição que encontra o valor velho dispara a sua, e o stampede volta pela porta dos fundos.
- Falha na revalidação não pode derrubar o valor antigo. Enquanto o TTL duro não expirar, servir dado velho é melhor do que não servir nada.
- O dicionário de semáforos por chave cresce indefinidamente se as chaves forem ilimitadas. Em produção, use um cache com limite de tamanho ou remova a entrada quando não houver mais ninguém esperando.
- Stale-while-revalidate troca frescor por disponibilidade. Para dado que não pode estar velho — saldo, estoque em conferência — a escolha é outra.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /experiment` | Limpa a chave, dispara N requisições simultâneas e conta as chamadas à origem |
| `GET /swr/{key}` | Stale-while-revalidate, informando se o valor servido era velho |
| `GET /jitter` | Amostras de TTL com e sem jitter |
| `POST /reset` | Limpa uma chave e zera o contador |
| `GET /stats` | Chamadas à origem acumuladas |

Resultado medido, com 30 requisições simultâneas e origem de 400ms:

| Estratégia | Requisições | Chamadas à origem | Valores distintos | Tempo total |
|---|---|---|---|---|
| Sem proteção | 30 | **30** | 2 | 428ms |
| Single-flight | 30 | **1** | 1 | 431ms |

Mesmo tempo para quem chamou; trinta vezes menos carga no banco. Os "2 valores distintos" da primeira linha são efeito colateral do stampede: várias respostas da origem sobrescreveram a chave, e requisições diferentes receberam versões diferentes do mesmo dado.

Ciclo do stale-while-revalidate (TTL curto de 3s, TTL duro de 10s):

```text
1. cache vazio        -> 412ms   valor A   (alguem espera, uma vez so)
2. logo em seguida    ->   0ms   valor A   fresco
3. apos 4s            ->   0ms   valor A   VELHO, dispara revalidacao
4. logo em seguida    ->   0ms   valor A   velho, NAO dispara outra
5. apos a revalidacao ->   0ms   valor B   fresco de novo

total de chamadas a origem nas 5 requisicoes: 2
```

Depois da primeira, ninguém mais esperou — nem quando o valor venceu.

Efeito do jitter sobre um TTL base de 10.000ms:

```text
sem jitter: 10000  10000  10000  10000  10000  10000
com jitter:  9450  11034   9214   8464  11532   9416
```

Sem jitter, chaves populadas no mesmo instante expiram no mesmo instante. Com ±20%, a expiração se espalha por quatro segundos.

Como escolher:

| Situação | Técnica |
|---|---|
| Chave popular, origem lenta, uma instância | Single-flight |
| Chave popular, várias instâncias | Single-flight + lock distribuído |
| Muitas chaves populadas ao mesmo tempo | Jitter no TTL |
| Latência não pode variar, dado tolera atraso | Stale-while-revalidate |
| Dado não pode estar desatualizado | Nenhuma das anteriores; aceite a espera |

Relação com os vizinhos da trilha: `CacheAside` e `CachePatterns` cobrem os padrões de leitura e escrita; este projeto trata do que acontece na expiração, que aqueles não abordam. `RedisDistributedLockDemo` fornece a peça que falta para o single-flight entre instâncias.

## Referências e documentação complementar

- https://en.wikipedia.org/wiki/Cache_stampede
- https://www.rfc-editor.org/rfc/rfc5861
- https://redis.io/docs/latest/develop/use/patterns/
- https://stackexchange.github.io/StackExchange.Redis/
