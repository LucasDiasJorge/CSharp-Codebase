# DistributedCacheInvalidationDemo

API que sincroniza a invalidação de caches locais entre várias instâncias usando Redis Pub/Sub, e mostra a divergência que aparece quando esse aviso não existe.

## Visão geral

Cache local é rápido porque não sai do processo. Esse é também o problema: quando uma instância escreve no banco, ela limpa o próprio cache e o do Redis, mas as outras instâncias continuam com a cópia antiga em memória. Elas não têm como saber que o dado mudou, e vão servir o valor velho até a entrada expirar.

O sintoma é desconcertante em produção: a mesma requisição devolve preços diferentes dependendo de qual réplica atendeu. Recarregar a página "conserta" às vezes — quando o balanceador cai numa instância que já foi reiniciada.

A correção é um canal de avisos. Quem escreve publica uma mensagem no Redis dizendo qual chave mudou; toda instância assina esse canal e, ao receber, remove a entrada do próprio cache local. O exemplo tem três nós com cache local independente, e permite escrever com e sem o aviso para comparar o resultado lado a lado.

A consistência resultante é **eventual**, e vale ser explícito: entre a escrita e a chegada da mensagem existe uma janela de milissegundos em que os nós divergem. Pub/Sub do Redis é fire-and-forget — um nó desconectado no momento da publicação simplesmente não recebe o aviso, e só se corrige quando o TTL expirar.

## Conceitos abordados

- Cache em dois níveis: L1 local por instância e L2 compartilhado no Redis.
- Divergência entre caches locais após uma escrita sem aviso.
- Redis Pub/Sub como canal de invalidação.
- Identificação do publicador para ignorar a própria mensagem.
- Invalidar em vez de reescrever, e por quê.
- Ordem entre gravar, invalidar e publicar.
- Consistência eventual e a janela de divergência.
- Fire-and-forget do Pub/Sub e o papel do TTL como rede de segurança.

## Objetivos de aprendizagem

- Reconhecer o sintoma de cache local dessincronizado entre réplicas.
- Implementar invalidação por evento sem transformar o cache em fonte de erro.
- Entender por que se publica a invalidação, e não o valor novo.
- Avaliar o que o Pub/Sub não garante e o que precisa cobrir essa lacuna.

## Estrutura do projeto

```text
DistributedCacheInvalidationDemo/
|-- Cache/
|   |-- CacheNode.cs
|   `-- NodeCluster.cs
|-- Data/
|   `-- ProductRepository.cs
|-- Properties/
|   `-- launchSettings.json
|-- DistributedCacheInvalidationDemo.csproj
|-- DistributedCacheInvalidationDemo.http
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
dotnet run --project 06-Caching/Caching/DistributedCacheInvalidationDemo/DistributedCacheInvalidationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 06-Caching/Caching/DistributedCacheInvalidationDemo/DistributedCacheInvalidationDemo.csproj
```

A API sobe em `http://localhost:5090` com três nós simulados — `A`, `B` e `C` —, cada um com cache local próprio e assinatura própria no Redis. Em produção cada nó seria um processo; aqui convivem no mesmo para o exemplo caber em um comando.

Roteiro que mostra a divergência:

```bash
curl -s -X POST http://localhost:5090/reset
curl -s -X POST http://localhost:5090/warmup/1

# escrita SEM aviso
curl -s -X PUT http://localhost:5090/nodes/A/products/1/no-invalidation \
  -H "Content-Type: application/json" -d '{"value":"Teclado - R$ 299,00"}'

curl -s http://localhost:5090/nodes/A/products/1   # novo valor
curl -s http://localhost:5090/nodes/B/products/1   # valor ANTIGO
curl -s http://localhost:5090/nodes/C/products/1   # valor ANTIGO
```

## Boas práticas e pontos de atenção

- Publique a **invalidação**, não o valor novo. Se dois nós escrevem em sequência e as mensagens chegam fora de ordem, quem publica valor grava o dado errado em todo mundo. Apagar a entrada faz o próximo leitor buscar na origem, e a ordem deixa de importar.
- Grave primeiro, invalide depois. Publicar antes de gravar abre uma janela em que outro nó recarrega o valor antigo e o recoloca no cache — o aviso chega e vai embora antes de haver o que invalidar.
- Identifique o publicador na mensagem. O nó que escreveu já removeu a entrada antes de publicar; ignorar a própria mensagem evita trabalho redundante e deixa os contadores legíveis.
- Mantenha o TTL mesmo com invalidação por evento. Pub/Sub do Redis é fire-and-forget: um nó que estava desconectado no instante da publicação nunca recebe o aviso, e o TTL é a única coisa que o corrige.
- Assuma a consistência eventual. Existe uma janela de milissegundos em que os nós divergem; para dado que não tolera isso, a saída é não manter cache local — ler sempre do L2, ou da origem.
- Cuidado com invalidação em massa. Apagar uma chave muito lida em todas as instâncias ao mesmo tempo produz um stampede — ver `CacheStampedeProtectionDemo`.
- Um canal para tudo é simples e barato até certo volume. Acima disso, vale separar canais por tipo de entidade, para que um nó não processe avisos de chaves que ele nunca cacheia.

## Conteúdo complementar

Arquitetura:

```text
       no A            no B            no C
      [L1 local]      [L1 local]      [L1 local]
           |               |               |
           +-------+-------+-------+-------+
                   |               |
              [Redis L2]     [canal cache:invalidation]
                   |               ^
              [banco]              |
                                escrita publica aqui
```

Endpoints:

| Rota | Finalidade |
|---|---|
| `GET /nodes/{no}/products/{id}` | Lê por um nó; informa se veio de L1, L2 ou origem |
| `PUT /nodes/{no}/products/{id}` | Escreve **e publica** a invalidação |
| `PUT /nodes/{no}/products/{id}/no-invalidation` | Escreve sem avisar, para comparação |
| `POST /warmup/{id}` | Aquece o L1 dos três nós |
| `GET /state` | Cache local e contadores de cada nó |
| `POST /reset` | Limpa tudo |

Resultado observado — escrita **sem** invalidação, pelo nó A:

| Nó | Valor servido | Origem |
|---|---|---|
| A | `R$ 299,00 (PROMOCAO)` | origem |
| B | `R$ 349,90` | **L1 (memória do nó)** |
| C | `R$ 349,90` | **L1 (memória do nó)** |

B e C continuam servindo o preço antigo, e continuariam até o TTL expirar.

Resultado com invalidação publicada:

| Nó | Valor servido | Origem |
|---|---|---|
| A | `R$ 279,00 (COM AVISO)` | origem |
| B | `R$ 279,00 (COM AVISO)` | L2 (Redis) |
| C | `R$ 279,00 (COM AVISO)` | L2 (Redis) |

Contadores após a publicação:

```json
{ "no": "A", "invalidacoesRecebidas": 0, "invalidacoesProprias": 1 }
{ "no": "B", "invalidacoesRecebidas": 1, "invalidacoesProprias": 0 }
{ "no": "C", "invalidacoesRecebidas": 1, "invalidacoesProprias": 0 }
```

A ignorou a própria mensagem; B e C aplicaram a invalidação e, na leitura seguinte, foram buscar no Redis.

O que o Pub/Sub garante e o que não garante:

| | Garantia |
|---|---|
| Entrega a quem está conectado | Sim |
| Entrega a quem está desconectado | **Não** — a mensagem se perde |
| Ordem entre mensagens de publicadores diferentes | Não |
| Confirmação de recebimento | Não |
| Persistência | Não |

É por isso que o TTL continua necessário: ele é a rede de segurança para todo aviso perdido. Onde a perda for inaceitável, o caminho é Redis Streams ou um broker com entrega garantida.

Relação com os vizinhos da trilha: `CacheAside` e `CachePatterns` cobrem os padrões de leitura e escrita em uma instância. `CacheStampedeProtectionDemo` trata da expiração sob concorrência. Este projeto cobre o que acontece entre instâncias, que nenhum dos dois aborda.

## Referências e documentação complementar

- https://redis.io/docs/latest/develop/interact/pubsub/
- https://redis.io/docs/latest/develop/reference/client-side-caching/
- https://stackexchange.github.io/StackExchange.Redis/PubSubOrder
- https://martinfowler.com/bliki/TwoHardThings.html
