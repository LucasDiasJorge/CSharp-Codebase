# ApiGatewayAggregationDemo

Gateway que compõe a resposta de três serviços em uma só, propagando correlation ID e tratando timeout e falha parcial sem derrubar a requisição inteira.

## Visão geral

Uma tela de dashboard precisa de perfil, pedidos e recomendações. Deixar o cliente fazer três chamadas expõe a topologia interna e multiplica a latência de rede; o gateway faz as três e devolve uma resposta.

A primeira decisão é chamar **em paralelo**. Sequencialmente, o custo é a soma das latências; em paralelo, é a do serviço mais lento. Com serviços de 120, 400 e 250ms, a diferença medida é 800ms contra 413ms — e ela cresce a cada serviço agregado.

A segunda é o que fazer quando um serviço não responde. Um gateway que devolve 500 porque as recomendações falharam joga fora o perfil e os pedidos, que vieram sem problema. O tratamento correto é responder 200 com o que se tem, marcando o que faltou, e reservar o erro para quando **nada** responder. Aqui, isso é 200 com `completo: false` no caso parcial e 503 só quando os três falham.

A terceira é o timeout por serviço. Sem ele, um serviço lento segura a resposta inteira pelo tempo que quiser — o exemplo corta em 1200ms um serviço que levaria 3000ms, e entrega o resto.

E, atravessando tudo, o correlation ID: gerado no gateway ou aceito do cliente, propagado por header a cada serviço, para que as quatro requisições que atenderam a mesma chamada possam ser juntadas no log.

## Conceitos abordados

- Agregação em paralelo e latência igual à do serviço mais lento.
- Falha parcial e degradação controlada.
- Distinção entre resposta incompleta e falha total.
- Timeout por serviço, com token ligado ao cancelamento do cliente.
- Propagação de correlation ID por header.
- Distinção entre "respondeu", "demorou demais" e "falhou".
- Composição de contratos: o cliente não vê a topologia de trás.

## Objetivos de aprendizagem

- Medir o ganho real de agregar em paralelo.
- Decidir o status HTTP de uma resposta parcial de forma consciente.
- Dimensionar o timeout do gateway em relação ao do cliente.
- Instrumentar uma chamada distribuída de modo a conseguir reconstruí-la depois.

## Estrutura do projeto

```text
ApiGatewayAggregationDemo/
|-- Downstream/
|   `-- DownstreamSimulator.cs
|-- Gateway/
|   `-- DashboardAggregator.cs
|-- Properties/
|   `-- launchSettings.json
|-- ApiGatewayAggregationDemo.csproj
|-- ApiGatewayAggregationDemo.http
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/ApiGatewayAggregationDemo/ApiGatewayAggregationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 08-ArchitecturalPatterns/ApiGatewayAggregationDemo/ApiGatewayAggregationDemo.csproj
```

A API sobe em `http://localhost:5298` e não exige serviço externo. Os três serviços de trás são endpoints do mesmo processo, chamados pelo gateway por HTTP de verdade — moram juntos apenas para o exemplo rodar com um comando.

Roteiro:

```bash
curl -s "http://localhost:5298/customers/c-1/dashboard"                 # paralelo
curl -s "http://localhost:5298/customers/c-1/dashboard?parallel=false"  # sequencial

curl -s -X POST http://localhost:5298/simulation/orders \
  -H "Content-Type: application/json" -d '{"behavior":"Slow"}'

curl -s "http://localhost:5298/customers/c-1/dashboard"                 # timeout no lento
```

## Boas práticas e pontos de atenção

- Chame em paralelo. Sequencial, cada serviço agregado soma a sua latência à resposta; em paralelo, só o mais lento conta.
- Resposta parcial é resposta. Devolver 500 porque um de três serviços falhou descarta o trabalho que deu certo. Responda com o que tem e diga o que faltou — o cliente decide o que fazer com isso.
- Reserve o erro para a falha total. Se nada respondeu, aí sim não há resposta a dar: 503.
- Timeout por serviço, e menor que o do cliente. Se o gateway espera mais do que quem o chamou, o cliente desiste primeiro e todo o trabalho é jogado fora.
- Ligue o timeout ao `RequestAborted`. Um token independente faz o gateway continuar trabalhando depois de o cliente ter ido embora.
- Distinga timeout de falha. São causas diferentes, com respostas operacionais diferentes — agrupar as duas em "erro" impede o diagnóstico.
- Aceite o correlation ID do cliente quando vier. Gerar um novo quebra o rastro de quem já estava rastreando a chamada desde antes.
- Devolva o correlation ID no header da resposta. Sem isso, o cliente não tem o que informar ao suporte.
- Cuidado para o gateway não virar um monólito distribuído. Ele compõe e traduz; se começar a conter regra de negócio, vira um ponto de acoplamento entre todos os serviços.
- Agregação é candidata natural a circuit breaker e cache. Um serviço que falha sempre não precisa ser chamado a cada requisição — ver `CircuitBreakerDemo` e a trilha `06-Caching`.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `GET /customers/{id}/dashboard` | Agregação; `?parallel=false` para comparar |
| `GET /services/{nome}/{id}` | Os três serviços de trás |
| `POST /simulation/{servico}` | Muda o comportamento (`Normal`, `Slow`, `Failing`) |
| `GET /simulation/correlations` | Correlation IDs que chegaram a cada serviço |
| `POST /simulation/reset` | Volta todos ao normal |

Latências normais, diferentes de propósito:

| Serviço | Latência |
|---|---|
| `profile` | 120ms |
| `orders` | 400ms |
| `recommendations` | 250ms |

Paralelo versus sequencial, medido com o processo aquecido:

| Modo | Tempo total | Corresponde a |
|---|---|---|
| Paralelo | ~413ms | O serviço mais lento (400ms) |
| Sequencial | ~800ms | A soma das três (770ms) |

Falha parcial — recomendações fora do ar:

```text
gateway status=200
completo: false
  profile          ok=true    134ms
  orders           ok=true    416ms
  recommendations  ok=false   260ms   erro: HTTP 500
```

O cliente recebe perfil e pedidos, e sabe exatamente o que faltou.

Timeout — pedidos levando 3000ms contra o limite de 1200ms:

```text
tempoTotalMs: 1224
completo: false
  profile          ok=true    125ms
  orders           ok=false  1224ms   erro: timeout apos 1200ms
  recommendations  ok=true    269ms
```

A resposta saiu em 1224ms, e não em 3000ms. O serviço lento foi cortado; os outros dois chegaram inteiros.

Falha total:

```text
gateway status=503
```

Propagação do correlation ID, enviando `X-Correlation-Id: rastreio-abc`:

```json
{
  "profile":         ["rastreio-abc"],
  "orders":          ["rastreio-abc"],
  "recommendations": ["rastreio-abc"]
}
```

O mesmo identificador chegou aos três serviços, o que permite juntar as quatro requisições no log.

Como decidir o status da resposta:

| Situação | Status | Racional |
|---|---|---|
| Todos responderam | 200 | Resposta completa |
| Parte respondeu | 200, com `completo: false` | Descartar o que deu certo é pior |
| Nenhum respondeu | 503 | Não há resposta a dar |
| Cliente desistiu | — | O gateway cancela junto, via `RequestAborted` |

Relação com os vizinhos da trilha: `CircuitBreakerDemo` trata de parar de chamar um serviço que falha sempre, o complemento natural de um agregador. `ModularMonolithCommerceDemo` mostra a alternativa de manter tudo em um processo, com fronteiras internas. `PortsAndAdapters` trata de fronteira entre aplicação e infraestrutura.

## Referências e documentação complementar

- https://microservices.io/patterns/apigateway.html
- https://learn.microsoft.com/azure/architecture/patterns/gateway-aggregation
- https://www.w3.org/TR/trace-context/
- https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory
