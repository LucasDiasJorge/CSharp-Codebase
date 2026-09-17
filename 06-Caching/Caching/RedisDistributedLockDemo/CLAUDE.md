# CLAUDE.md — RedisDistributedLockDemo

Lock distribuído sobre Redis com token de propriedade, renovação de lease e fencing token. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 06-Caching/Caching/RedisDistributedLockDemo/RedisDistributedLockDemo.csproj
dotnet run --project 06-Caching/Caching/RedisDistributedLockDemo/RedisDistributedLockDemo.csproj
```

**Exige Redis** em `localhost:6379`:

```bash
docker run -d --name cbk-redis -p 6379:6379 redis:7-alpine
```

Sobe em `http://localhost:5091` (porta trocada do padrão do template, que colidia com [CacheStampedeProtectionDemo](../CacheStampedeProtectionDemo/CLAUDE.md) em 5001). Os três cenários estão no README.

## Estrutura interna

`Locking/DistributedLock` tem dois scripts Lua, e **os dois precisam continuar sendo Lua**:

- `ReleaseScript` compara o token antes do `DEL`. `GET` + `DEL` como comandos separados não são atômicos: entre eles o lease pode expirar e outro processo adquirir o lock, e o `DEL` apagaria o lock alheio.
- `RenewScript` só faz `PEXPIRE` se o token conferir. Um `PEXPIRE` cego estenderia o lock de outro processo.

`ReleaseUnsafeAsync` é o contraexemplo (`DEL` direto) e existe para o endpoint `/lock/release-unsafe`. **Não remover** — é material didático.

`Work/ProtectedResource` conta quantos trabalhadores estão na seção crítica ao mesmo tempo. É esse contador que transforma a falha do lock em número observável. Também guarda `_lastAcceptedFence` e rejeita escrita com fencing menor.

`Work/WorkerRunner` aceita `stallFirstWorker`: pausa **só do worker-1**, entre terminar o trabalho e gravar. Reproduz a pausa de GC do diagrama do Kleppmann — sem ela o fencing token nunca é exercitado, porque o trabalhador velho sempre termina antes do novo e sua escrita é aceita normalmente. **A primeira versão deste sample tinha esse buraco**: o fencing existia no código mas nenhum cenário o acionava.

## Pontos de atenção

- TFM `net10.0`. Pacote: `StackExchange.Redis` 3.2.1.
- **Serviço externo obrigatório:** Redis. Sem ele, `ConnectionMultiplexer.Connect` falha no start.
- Os números das três linhas da tabela do README foram medidos; alterar `ttlMs`, `workMs` ou `stallFirstWorkerMs` nos exemplos muda o resultado. A relação que precisa continuar valendo: **A** viola sem renovação, **B** não viola com renovação, **C** viola mas o fencing rejeita 1 escrita.
- O cenário C depende de `stallFirstWorkerMs > workMs` para que a escrita do worker-1 chegue depois da do worker-2. Reduzir a pausa faz a rejeição sumir e o exemplo perder o ponto.
- `RenewPeriodicallyAsync` renova a **TTL/3**. Renovar na borda do TTL não deixa margem para retransmissão de rede; não aumentar esse divisor.
- Ao perder o lease, o renovador só registra e sai — o trabalho continua. Isso é **deliberado**, para a violação ficar observável. Em produção o trabalho deveria ser abortado, e está dito assim no comentário e no README.
- O lock é de nó único. Redlock (vários nós Redis) não está implementado e não resolveria pausa de processo nem desvio de relógio; a discussão está referenciada no README.
- **Fronteira com os vizinhos**: [CacheStampedeProtectionDemo](../CacheStampedeProtectionDemo/CLAUDE.md) faz single-flight em memória, que só coordena uma instância — este projeto é a peça para coordenar entre processos. `CacheIncrement` e `RedisHashFieldExpire` cobrem outras operações atômicas.
