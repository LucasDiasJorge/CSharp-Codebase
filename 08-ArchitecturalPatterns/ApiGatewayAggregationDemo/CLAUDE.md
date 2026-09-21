# CLAUDE.md — ApiGatewayAggregationDemo

Gateway que agrega três serviços em paralelo, com correlation ID, timeout por serviço e resposta parcial. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/ApiGatewayAggregationDemo/ApiGatewayAggregationDemo.csproj
dotnet run --project 08-ArchitecturalPatterns/ApiGatewayAggregationDemo/ApiGatewayAggregationDemo.csproj
```

Sobe em `http://localhost:5298`. Sem serviço externo. Roteiro em `ApiGatewayAggregationDemo.http`.

## Estrutura interna

Os três serviços de trás são endpoints **do mesmo processo**, mas o gateway os chama por **HTTP de verdade** (`HttpClient` com `BaseAddress` apontando para si mesmo). Não há atalho em memória — é isso que torna timeout, falha e propagação de header reais.

`Gateway/DashboardAggregator.AggregateAsync` tem o parâmetro `parallel` só para o cenário de comparação; o caminho normal é paralelo.

`CallAsync` usa `CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)` + `CancelAfter`. **O linked token é necessário:** um token independente faria o gateway continuar trabalhando depois de o cliente desistir. O `catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)` distingue timeout do gateway de cancelamento do cliente.

`FragmentResult` separa "respondeu", "timeout" e "falhou" — agrupar em "erro" impediria o diagnóstico.

## Pontos de atenção

- TFM `net10.0`. **Sem pacote externo** — `HttpClient`, `IHttpClientFactory` e `System.Text.Json` são do framework.
- **Armadilha já corrigida:** `ConfigureHttpJsonOptions` registra `JsonStringEnumConverter`. Sem ele, `System.Text.Json` só aceita enum como **número**, e `{"behavior":"Slow"}` — exatamente o que o README e o `.http` documentam — responde 400. Removê-lo quebra todos os cenários de simulação de uma vez.
- `Gateway:BaseAddress` aponta para `http://localhost:5298`, a **própria** aplicação. Se a porta do `launchSettings.json` mudar, o gateway passa a chamar o vazio e tudo vira timeout. Os dois precisam andar juntos.
- `PerServiceTimeout` é 1200ms e `SlowLatencyMs` é 3000ms. A relação (lento > timeout) é o que faz o cenário funcionar; as latências normais (120/400/250) sustentam a comparação paralelo vs sequencial no README.
- Os tempos citados (~413ms paralelo, ~800ms sequencial) foram medidos **com o processo aquecido**. A primeira requisição depois do start custa mais, por causa da inicialização do `HttpClient` — ao conferir, descarte a primeira.
- 200 com `completo: false` para resposta parcial e 503 só na falha total é **decisão deliberada**, comentada no código e explicada no README. Não "padronizar" para 500 em qualquer falha.
- Circuit breaker e cache **não** estão implementados: são o passo seguinte natural de um agregador e estão citados como fronteira (`CircuitBreakerDemo`, trilha `06-Caching`).
- **Fronteira com os vizinhos**: `CircuitBreakerDemo` cobre parar de chamar quem falha sempre. [ModularMonolithCommerceDemo](../ModularMonolithCommerceDemo/CLAUDE.md) mostra a alternativa de um processo só com fronteiras internas. Aqui o assunto é composição de respostas sob falha parcial.
