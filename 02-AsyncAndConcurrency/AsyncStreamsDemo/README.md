# AsyncStreamsDemo

Projeto console sobre `IAsyncEnumerable<T>`: como escrever um iterador assincrono com `yield return`, consumi-lo com `await foreach` e cancelar a enumeracao de dados que chegam progressivamente.

## Visao geral

Um metodo `async IAsyncEnumerable<T>` combina as duas mecanicas: a maquina de estados do iterador, que pausa em cada `yield return`, e a do `async`, que pausa em cada `await`. O resultado e uma sequencia cujos itens ficam disponiveis um a um, conforme sao produzidos, em vez de so no fim.

O exemplo mede essa diferenca. O cenario 1 coleta as mesmas seis leituras de duas formas: como stream e como `Task<List<T>>`. O tempo total e praticamente identico — o que muda e que o stream entrega a primeira leitura em ~155ms, enquanto a lista so devolve alguma coisa depois de ~940ms.

O cenario 3 e o mais importante do conjunto. Ele mostra que o `CancellationToken` **nao chega sozinho** dentro de um iterador assincrono: sem o atributo `[EnumeratorCancellation]` no parametro, o token entregue por `WithCancellation` e simplesmente ignorado, e o stream continua produzindo depois de o consumidor ter pedido para parar.

## Conceitos abordados

- Iteradores assincronos com `async IAsyncEnumerable<T>` e `yield return`.
- Consumo com `await foreach` e entrega item a item.
- `[EnumeratorCancellation]` e `WithCancellation` como o par que faz o cancelamento funcionar.
- `try/finally` dentro do iterador e `DisposeAsync` disparado por `break`.
- Paginacao exposta como stream unico de itens.
- Operadores preguiçosos proprios sobre `IAsyncEnumerable<T>`.
- Re-enumeracao e o custo de repetir I/O.

## Objetivos de aprendizagem

- Escrever um iterador assincrono e reconhecer quando ele vale mais que devolver a colecao pronta.
- Fazer o cancelamento chegar de fato ao corpo do iterador.
- Entender por que `break` no consumidor evita trabalho que ainda nao foi feito na fonte.
- Compor operadores sobre um stream assincrono sem materializar coleções intermediarias.
- Decidir quando materializar o stream para nao pagar o I/O duas vezes.

## Estrutura do projeto

```text
AsyncStreamsDemo/
|-- Demo/
|   `-- AsyncStreamsDemoRunner.cs
|-- Models/
|   `-- SensorReading.cs
|-- Sources/
|   |-- PagedCatalogClient.cs
|   `-- SensorFeed.cs
|-- AsyncOperators.cs
|-- AsyncStreamsDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 02-AsyncAndConcurrency/AsyncStreamsDemo/AsyncStreamsDemo.csproj
```

Para validar compilacao:

```bash
dotnet build 02-AsyncAndConcurrency/AsyncStreamsDemo/AsyncStreamsDemo.csproj
```

A execucao completa leva cerca de 12 segundos e nao exige servico externo.

**O build emite um aviso `CS8425`, e isso e proposital.** Ele aponta o metodo `SensorFeed.StreamIgnoringCancellationAsync`, a versao deliberadamente quebrada usada no cenario 3. O proprio compilador avisa que o token nao sera consumido — o aviso faz parte da licao e nao deve ser silenciado.

## Boas praticas e pontos de atencao

- Anote o parametro de cancelamento com `[EnumeratorCancellation]` sempre que o iterador tiver um. Sem isso o `WithCancellation` do consumidor nao tem efeito, e o compilador avisa com `CS8425`.
- Repasse o token para as chamadas assincronas internas. O atributo faz o token chegar ao parametro; quem para a producao e o `await` que o recebe.
- Use `try/finally` para liberar recursos no iterador. Nao e possivel usar `catch` em volta de um `yield return`, mas `finally` funciona e roda tambem quando o consumidor sai por `break`.
- Prefira `IAsyncEnumerable<T>` a `Task<List<T>>` quando o consumidor puder comecar a trabalhar antes do ultimo item, ou quando a sequencia for grande ou infinita.
- Um `IAsyncEnumerable<T>` nao guarda resultado: cada enumeracao refaz todo o trabalho. Se a sequencia for consumida mais de uma vez, materialize antes.
- `await foreach` sobre um stream que veio de outra camada aceita `WithCancellation(token)` e `ConfigureAwait(false)` — os dois podem ser encadeados.
- Existe o pacote `System.Linq.Async` com operadores prontos. Os operadores deste projeto sao escritos a mao so para deixar a mecanica visivel.

## Conteudo complementar

Resultados de uma execucao de referencia:

| # | Cenario | Medicao | Resultado |
|---|---------|---------|-----------|
| 1 | Entrega progressiva | tempo ate o 1o item | `await foreach` 155ms · `Task<List<T>>` 938ms |
| 2 | Paginacao preguicosa | paginas buscadas | consumo completo 5 · `break` no 5o item 2 |
| 3 | Cancelamento | leituras consumidas de 8 | com o atributo 3 · sem o atributo 8 |
| 4 | Composicao preguicosa | leituras produzidas de 20 | 6, para entregar 3 itens |
| 5 | Re-enumeracao | paginas buscadas | duas enumeracoes 10 · materializado 5 |

No cenario 1 o tempo **total** e praticamente igual nos dois casos (958ms contra 938ms). O ganho do stream nao esta em terminar antes, e sim em comecar antes.

No cenario 4 o pipeline `WhereAsync(par).SelectAsync(formatar).TakeAsync(3)` faz a fonte produzir apenas 6 das 20 leituras: o `yield break` do `TakeAsync` encerra a cadeia inteira, e o `finally` do `SensorFeed` roda na sequencia.

As duas mecanicas de um iterador assincrono:

```text
yield return  -> pausa a producao e devolve o item ao consumidor
await         -> pausa a producao e devolve a thread ao pool
[EnumeratorCancellation] -> liga o token do consumidor ao parametro do iterador
break/yield break        -> encerra a enumeracao e dispara DisposeAsync
```

Relacao com os vizinhos: `01-Fundamentals/YieldReturnDemo` cobre o iterador **sincrono** — maquina de estados, execucao adiada e armadilhas de `IEnumerable<T>`. Este projeto e a contraparte assincrona e assume aquele conteudo como base.

## Referencias e documentacao complementar

- https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements#await-foreach
- https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1
- https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.enumeratorcancellationattribute
- https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream
