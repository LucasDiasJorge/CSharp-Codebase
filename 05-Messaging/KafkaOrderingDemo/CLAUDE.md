# CLAUDE.md — KafkaOrderingDemo

API que demonstra chave, partição e consumer group determinando ordem, distribuição e teto de paralelismo no Kafka. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 05-Messaging/KafkaOrderingDemo/KafkaOrderingDemo.csproj
dotnet run --project 05-Messaging/KafkaOrderingDemo/KafkaOrderingDemo.csproj
```

**Exige Kafka** em `localhost:9092`. **O `docker-compose.yml` de `05-Messaging/Kafka/` NÃO funciona mais** — `bitnami/kafka:3.4` foi removido do Docker Hub. Use o `docker run` com `apache/kafka:3.9.0` em modo KRaft que está no README (verificado).

Sobe em `http://localhost:5186` e cria o tópico `pedidos` com 3 partições no start.

## Estrutura interna

`Kafka/OrderProducer` publica e devolve `Partition`/`Offset` de cada mensagem — é o que torna o mapeamento chave→partição visível. `Acks.All` e `EnableIdempotence` estão ligados de propósito e comentados no código.

`Kafka/GroupConsumerRunner` sobe N consumidores no **mesmo `GroupId`** e relata as partições atribuídas a cada um. É o que demonstra o teto de paralelismo. `AutoOffsetReset.Earliest` faz um grupo novo ler do início, o que torna o exemplo repetível — **por isso cada execução precisa de um `group` novo**, senão os offsets já estão commitados... na prática não, porque `EnableAutoCommit = false`; ainda assim, reusar o nome do grupo entre execuções deixa o resultado confuso.

`consumer.Close()` no `finally` faz a saída limpa do grupo; sem ele o rebalance espera o `SessionTimeoutMs`.

## Pontos de atenção

- TFM `net10.0`. Pacote: `Confluent.Kafka` 2.15.1.
- **Serviço externo obrigatório:** Kafka. O `EnsureTopicAsync` no start falha se o broker não responder.
- **A janela de consumo é por tempo**, não por quantidade: `/consume` roda por `seconds` (padrão 8) e devolve o que conseguiu ler. Em máquina lenta, aumentar o valor evita relatório vazio. Os números do README foram obtidos com 9 segundos.
- **Resultados dependentes do ambiente:** a partição de cada chave vem do hash (`cliente-A` caiu na 2, `cliente-B` na 0) e a divisão entre consumidores depende do rebalance. O README apresenta os valores como observados, não como garantidos. O que é estável e deve continuar valendo: uma chave sempre em uma única partição, e um consumidor ocioso quando `consumers > partitions`.
- Mudar `Kafka:PartitionCount` depois do tópico criado **não** reparticiona nada — o tópico já existe. Para testar outro número, apague o tópico ou recrie o container.
- O número 3 de partições aparece em `KafkaSettings`, no README (várias tabelas) e no `.http`. Alterar exige atualizar os três.
- Os endpoints de consumo **bloqueiam** pela duração pedida. Ao testar via curl, use `--max-time` acima de `seconds`.
- **Fronteira com os vizinhos**: `Kafka/Send` e `Kafka/Receive` cobrem produzir/consumir básico; `KafkaStreamApi` cobre processamento de stream. Aqui o assunto é só particionamento e grupos — não expandir para transformação de stream.
