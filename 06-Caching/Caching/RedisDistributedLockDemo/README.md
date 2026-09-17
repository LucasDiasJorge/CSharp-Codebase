# RedisDistributedLockDemo

API que implementa lock distribuído sobre Redis com token de propriedade, renovação de lease e fencing token — e mostra, com números, o que acontece quando o lease expira no meio do trabalho.

## Visão geral

Um lock distribuído sobre Redis é um `SET chave token NX PX ttl`: uma operação atômica que só tem sucesso se ninguém mais tiver a chave. O TTL é obrigatório — sem ele, um processo que morre segurando o lock trava o recurso para sempre.

Mas o TTL cria o problema central. Se o trabalho demorar mais que o lease, o Redis apaga a chave, **outro processo adquire o lock, e os dois passam a executar a seção crítica ao mesmo tempo** — sem que nenhum dos dois perceba. É a falha que este exemplo reproduz e mede: um contador registra quantos trabalhadores estão dentro simultaneamente.

Duas defesas, com alcances diferentes. A renovação periódica do lease (watchdog) estende o TTL enquanto o trabalho continua, e resolve o caso comum de trabalho mais longo que o previsto. Mas não resolve o caso em que o processo *para* — uma pausa de GC, um swap, uma partição de rede: ele não renova, perde o lock, e ao voltar acha que ainda o tem.

Para isso existe o fencing token: um número monotônico entregue junto com o lock. O recurso protegido guarda o maior número que já aceitou e rejeita qualquer escrita com número menor. O lock pode falhar; a integridade do dado, não. É a diferença entre tratar lock distribuído como otimização e como garantia.

## Conceitos abordados

- `SET key token NX PX ttl` como aquisição atômica.
- Token de propriedade aleatório, e por que um lock sem ele é inseguro.
- Liberação com script Lua comparando o token antes de apagar.
- Renovação de lease com Lua, só se ainda formos o dono.
- Watchdog renovando a uma fração do TTL.
- Expiração de lease no meio do trabalho e violação de exclusão mútua.
- Fencing token monotônico e rejeição de escrita atrasada.
- Limitações do lock distribuído: pausas, relógio e partição de rede.

## Objetivos de aprendizagem

- Implementar aquisição, renovação e liberação sem janelas de corrida.
- Entender por que liberar com `GET` + `DEL` separados é inseguro.
- Reconhecer que renovar lease resolve lentidão, não pausa de processo.
- Usar fencing token quando a correção do dado não pode depender do lock.
- Dimensionar o TTL entre travar o recurso e perdê-lo cedo demais.

## Estrutura do projeto

```text
RedisDistributedLockDemo/
|-- Locking/
|   `-- DistributedLock.cs
|-- Work/
|   |-- ProtectedResource.cs
|   `-- WorkerRunner.cs
|-- Properties/
|   `-- launchSettings.json
|-- Program.cs
|-- README.md
`-- RedisDistributedLockDemo.csproj
```

## Como executar

Requer Redis:

```bash
docker run -d --name cbk-redis -p 6379:6379 redis:7-alpine
```

Depois:

```bash
dotnet run --project 06-Caching/Caching/RedisDistributedLockDemo/RedisDistributedLockDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 06-Caching/Caching/RedisDistributedLockDemo/RedisDistributedLockDemo.csproj
```

A API sobe em `http://localhost:5091`.

Os três cenários:

```bash
# A) lease de 1s, trabalho de 3s, sem renovacao -> exclusao mutua violada
curl -s -X POST http://localhost:5091/scenario -H "Content-Type: application/json" \
  -d '{"resource":"r1","workers":2,"ttlMs":1000,"workMs":3000,"autoRenew":false}'

# B) mesmo cenario, com renovacao -> nenhuma violacao
curl -s -X POST http://localhost:5091/scenario -H "Content-Type: application/json" \
  -d '{"resource":"r2","workers":2,"ttlMs":1000,"workMs":3000,"autoRenew":true}'

# C) worker-1 trava 3s antes de gravar -> fencing token rejeita a escrita atrasada
curl -s -X POST http://localhost:5091/scenario -H "Content-Type: application/json" \
  -d '{"resource":"r3","workers":2,"ttlMs":1000,"workMs":1500,"autoRenew":false,"stallFirstWorkerMs":3000}'
```

## Boas práticas e pontos de atenção

- Sempre use TTL. Um lock sem expiração vira um recurso travado permanentemente assim que um processo morre segurando-o.
- Guarde um token aleatório como valor do lock. Sem ele não há como distinguir "meu lock" do "lock de quem veio depois", e toda liberação vira um risco.
- Libere com script Lua. `GET` seguido de `DEL` não é atômico: entre os dois comandos o lease pode expirar e outro processo adquirir o lock — e o seu `DEL` apaga o lock **dele**.
- Renove com Lua também. Um `PEXPIRE` cego estende o lock de quem quer que o tenha, o que é pior do que não renovar.
- Renove a uma fração do TTL, não na borda. Renovar no último instante não deixa margem para uma retransmissão de rede ou uma pausa de coletor.
- Ao perder o lease, **aborte o trabalho**. Continuar significa operar sem exclusão mútua, que é exatamente o que o lock existia para impedir.
- Use fencing token quando a correção importar. Renovação reduz a chance de duas execuções simultâneas; não elimina. Se o dado não pode ser corrompido, o recurso protegido precisa rejeitar escritas velhas.
- Dimensionar o TTL é escolher entre dois erros: curto demais perde o lock durante trabalho normal; longo demais mantém o recurso travado depois de uma queda. Renovação permite TTL curto com trabalho longo.
- Redlock em vários nós Redis não resolve pausa de processo nem desvio de relógio. A discussão é pública e vale ler antes de confiar em lock distribuído para garantir integridade.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /scenario` | Roda N trabalhadores e mede violações de exclusão mútua |
| `POST /lock/acquire` | Adquire manualmente; devolve token e fencing token |
| `POST /lock/release` | Libera conferindo o token |
| `POST /lock/release-unsafe` | `DEL` sem conferir, para mostrar o estrago |
| `GET /lock/{resource}` | Estado atual do lock e TTL restante |
| `GET /resource` | Contadores e registro da seção crítica |

Resultados medidos nos três cenários:

| Cenário | TTL | Trabalho | Renovação | Máx. simultâneo | Violações | Escritas aceitas | Rejeitadas por fencing |
|---|---|---|---|---|---|---|---|
| A | 1000ms | 3000ms | não | **2** | **1** | 2 | 0 |
| B | 1000ms | 3000ms | sim | 1 | 0 | 2 | 0 |
| C | 1000ms | 1500ms + pausa de 3000ms | não | **2** | **1** | 1 | **1** |

No cenário A, o lease expira no meio do trabalho e os dois trabalhadores ficam na seção crítica ao mesmo tempo. No B, a renovação mantém o lock com o dono e nada se sobrepõe. No C, a exclusão mútua é violada do mesmo jeito — mas a escrita atrasada do `worker-1` é **rejeitada pelo fencing token**, e o dado permanece correto.

Sequência observada no cenário C:

```text
worker-1 entrou na secao critica
VIOLACAO: worker-2 entrou com 2 trabalhadores na secao critica
worker-2 escreveu 'resultado de worker-2' com fencing 2
worker-2 saiu da secao critica
REJEITADO: worker-1 tentou escrever com fencing 1, menor que 2
worker-1 saiu da secao critica
```

O lock falhou; o fencing token salvou a integridade.

Por que a liberação precisa de Lua:

```text
sem verificacao de token:
  t0  processo A adquire o lock (ttl 1s)
  t1  A fica lento; o lease expira
  t2  processo B adquire o lock
  t3  A termina e chama DEL  -> apaga o lock de B
  t4  processo C adquire o lock -> B e C rodam juntos

com verificacao de token:
  t3  A chama o script; o token nao confere; nada e apagado
```

O que cada defesa cobre:

| Falha | TTL | Renovação | Fencing token |
|---|---|---|---|
| Processo morre segurando o lock | Resolve | — | — |
| Trabalho mais longo que o previsto | — | Resolve | — |
| Pausa de GC / swap / partição de rede | — | Não resolve | **Resolve a integridade** |
| Desvio de relógio entre nós | — | Não resolve | **Resolve a integridade** |

Renovação reduz a frequência do problema. Só o fencing token garante que uma escrita velha não sobrescreva uma nova.

Relação com os vizinhos da trilha: `CacheStampedeProtectionDemo` usa single-flight com semáforo em memória, que coordena apenas uma instância — este projeto fornece a peça que falta para coordenar entre processos. `CacheIncrement` e `RedisHashFieldExpire` cobrem outras operações atômicas do Redis.

## Referências e documentação complementar

- https://redis.io/docs/latest/develop/use/patterns/distributed-locks/
- https://martin.kleppmann.com/2016/02/08/how-to-do-distributed-locking.html
- http://antirez.com/news/101
- https://redis.io/docs/latest/commands/set/
