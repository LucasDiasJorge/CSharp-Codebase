# KafkaOrderingDemo

API que demonstra como chave, partição e consumer group determinam ordenação, distribuição de carga e teto de paralelismo no Kafka.

## Visão geral

A frase mais repetida sobre o Kafka — "o Kafka garante ordem" — está incompleta a ponto de ser errada. O Kafka garante ordem **dentro de uma partição**, nunca no tópico inteiro. E quem escolhe a partição é a chave da mensagem.

Com chave, o produtor aplica um hash sobre ela e sempre chega à mesma partição. Todos os eventos de `cliente-A` acabam juntos, na ordem em que foram produzidos. Sem chave, o produtor distribui entre as partições, e dois eventos consecutivos podem ir para partições diferentes e ser lidos por consumidores diferentes, em qualquer ordem. Não existe configuração que conserte isso depois — a decisão é feita na publicação.

Do outro lado, o consumer group divide as partições entre os consumidores: cada partição vai para exatamente um consumidor do grupo. Isso dá paralelismo, mas com um teto rígido — **o número de partições**. Um grupo com quatro consumidores em um tópico de três partições tem três trabalhando e um parado, sem receber nada.

O exemplo deixa produzir com e sem chave, ver a partição atribuída a cada mensagem, subir N consumidores no mesmo grupo e verificar se a ordem por chave sobreviveu ao consumo paralelo.

## Conceitos abordados

- Ordem garantida por partição, não por tópico.
- Chave da mensagem como seletor de partição.
- Produção sem chave e perda da ordem relativa.
- Consumer group e atribuição de partições entre consumidores.
- Número de partições como teto do paralelismo do grupo.
- Grupos diferentes recebendo cada um uma cópia completa.
- `EnableIdempotence` e o reordenamento que uma retentativa pode causar.
- `Acks.All` e perda de mensagem em troca de líder.
- `Flush` antes de encerrar o produtor.

## Objetivos de aprendizagem

- Escolher a chave a partir da unidade que precisa de ordem.
- Dimensionar partições sabendo que elas limitam o paralelismo.
- Reconhecer que aumentar consumidores além das partições não acelera nada.
- Entender por que ordem e paralelismo são objetivos em tensão.
- Configurar o produtor para não perder nem reordenar mensagens em falha.

## Estrutura do projeto

```text
KafkaOrderingDemo/
|-- Kafka/
|   |-- GroupConsumerRunner.cs
|   |-- KafkaSettings.cs
|   `-- OrderProducer.cs
|-- Properties/
|   `-- launchSettings.json
|-- KafkaOrderingDemo.csproj
|-- KafkaOrderingDemo.http
|-- Program.cs
`-- README.md
```

## Como executar

Requer Kafka. O `docker-compose.yml` de `05-Messaging/Kafka/` aponta para uma imagem que não existe mais no Docker Hub; use o comando abaixo, que sobe um broker em modo KRaft (sem ZooKeeper):

```bash
docker run -d --name cbk-kafka --hostname cbk-kafka -p 9092:9092 \
  -e KAFKA_NODE_ID=1 -e KAFKA_PROCESS_ROLES=broker,controller \
  -e KAFKA_LISTENERS='PLAINTEXT://:9092,CONTROLLER://:9093' \
  -e KAFKA_ADVERTISED_LISTENERS='PLAINTEXT://localhost:9092' \
  -e KAFKA_LISTENER_SECURITY_PROTOCOL_MAP='PLAINTEXT:PLAINTEXT,CONTROLLER:PLAINTEXT' \
  -e KAFKA_INTER_BROKER_LISTENER_NAME=PLAINTEXT \
  -e KAFKA_CONTROLLER_QUORUM_VOTERS='1@localhost:9093' \
  -e KAFKA_CONTROLLER_LISTENER_NAMES=CONTROLLER \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  -e KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR=1 \
  -e KAFKA_TRANSACTION_STATE_LOG_MIN_ISR=1 \
  -e KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS=0 \
  apache/kafka:3.9.0
```

Depois:

```bash
dotnet run --project 05-Messaging/KafkaOrderingDemo/KafkaOrderingDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 05-Messaging/KafkaOrderingDemo/KafkaOrderingDemo.csproj
```

A API sobe em `http://localhost:5186` e cria o tópico `pedidos` com 3 partições no start.

Roteiro:

```bash
curl -s -X POST http://localhost:5186/produce/keyed -H "Content-Type: application/json" \
  -d '{"key":"cliente-A","count":5}'
curl -s -X POST http://localhost:5186/produce/keyed -H "Content-Type: application/json" \
  -d '{"key":"cliente-B","count":5}'
curl -s -X POST http://localhost:5186/produce/keyless -H "Content-Type: application/json" \
  -d '{"count":6}'

# 4 consumidores para 3 particoes: um fica ocioso
curl -s -X POST http://localhost:5186/consume -H "Content-Type: application/json" \
  -d '{"group":"g4","consumers":4,"seconds":9}'
```

Cada execução de `/consume` usa o `group` informado; repita com um nome novo para ler o tópico do início outra vez.

## Boas práticas e pontos de atenção

- Escolha a chave pela unidade que precisa de ordem. Se os eventos de um pedido precisam ser processados em sequência, a chave é o id do pedido — não o id do cliente nem um GUID por mensagem.
- Chave nula não é neutra: é a decisão de abrir mão da ordem. Às vezes é o que se quer, mas precisa ser consciente.
- Dimensione as partições antes. Elas limitam o paralelismo do grupo, e aumentar o número depois **muda o mapeamento de chaves para partições** — chaves passam a cair em outra partição, e a ordem em relação ao histórico se perde.
- Não adicione consumidores além do número de partições esperando ganho. Eles entram no grupo, participam do rebalance e ficam parados.
- Cuidado com a chave desbalanceada. Se 90% do tráfego tem a mesma chave, uma partição recebe 90% da carga e o paralelismo vira ficção.
- Ligue `EnableIdempotence`. Sem ela, uma retentativa interna do produtor pode reordenar mensagens da mesma partição — justamente no momento de instabilidade em que a ordem mais importa.
- Use `Acks.All` quando a perda for inaceitável. Com `acks=1`, uma troca de líder pode descartar mensagens já confirmadas ao produtor.
- Chame `Flush` antes de descartar o produtor. Mensagens ainda em buffer somem sem erro.
- Chame `Close` no consumidor ao sair. Sem isso o grupo só percebe a saída quando o `session.timeout` estoura, e o rebalance demora.
- Grupos diferentes são independentes: cada um recebe uma cópia completa do tópico. Isso é fan-out, não balanceamento.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `GET /topic` | Metadados e número de partições |
| `POST /produce/keyed` | Publica N eventos com a mesma chave |
| `POST /produce/keyless` | Publica N eventos sem chave |
| `POST /consume` | Sobe N consumidores num grupo e relata a divisão |
| `POST /consume/check-order` | Verifica se a ordem por chave se manteve |

Resultado observado ao produzir com chave, em tópico de 3 partições:

```text
cliente-A  -> particao 2, offsets 0 1 2 3 4     (cinco eventos, mesma particao)
cliente-B  -> particao 0, offsets 0 1 2 3 4
sem chave  -> particoes 2, 2, 1, 1, 1, 2        (espalhadas)
```

As chaves caíram em partições diferentes por acaso do hash — o que importa é que cada chave é **consistente** consigo mesma.

Distribuição das partições conforme o tamanho do grupo:

| Consumidores | Divisão observada | Ociosos |
|---|---|---|
| 1 | `[0,1,2]` | 0 |
| 2 | `[0,1]` e `[2]` | 0 |
| 3 | `[2]`, `[1]`, `[0]` | 0 |
| 4 | `[0]`, `[]`, `[2]`, `[1]` | **1** |

Com quatro consumidores, um recebe partição nenhuma e lê zero mensagens. Partição é a unidade de paralelismo: não há como dois consumidores do mesmo grupo dividirem uma.

Verificação de ordem com três consumidores em paralelo:

```json
[
  { "chave": "cliente-A", "particoes": [2], "mensagens": 5, "ordemPreservada": true },
  { "chave": "cliente-B", "particoes": [0], "mensagens": 5, "ordemPreservada": true }
]
```

Mesmo lendo em paralelo, cada chave chega ordenada — porque cada uma vive em uma única partição e cada partição tem um só consumidor.

O que é e o que não é garantido:

| Afirmação | Verdade? |
|---|---|
| Mensagens de uma partição chegam na ordem em que foram gravadas | Sim |
| Mensagens de uma mesma chave chegam em ordem | Sim, enquanto o número de partições não mudar |
| Mensagens do tópico chegam em ordem global | Não |
| Mais consumidores no grupo = mais vazão | Só até o número de partições |
| Dois grupos dividem a carga entre si | Não; cada grupo recebe tudo |

Relação com os vizinhos da trilha: `Kafka/Send` e `Kafka/Receive` cobrem o básico de produzir e consumir, e `KafkaStreamApi` trata de processamento de stream. Este projeto isola a mecânica de particionamento que os três dependem sem explicitar.

## Referências e documentação complementar

- https://kafka.apache.org/documentation/#intro_concepts_and_terms
- https://kafka.apache.org/documentation/#design_consumerposition
- https://docs.confluent.io/kafka-clients/dotnet/current/overview.html
- https://kafka.apache.org/documentation/#producerconfigs_enable.idempotence
