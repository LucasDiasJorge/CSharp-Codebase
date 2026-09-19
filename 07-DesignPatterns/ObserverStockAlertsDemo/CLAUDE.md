# CLAUDE.md — ObserverStockAlertsDemo

Console com o padrão Observer à mão, o equivalente com `event` do C#, e as armadilhas do padrão. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/ObserverStockAlertsDemo/ObserverStockAlertsDemo.csproj
dotnet run --project 07-DesignPatterns/ObserverStockAlertsDemo/ObserverStockAlertsDemo.csproj
```

Roda cinco cenários e termina. Sem serviço externo.

## Estrutura interna

`Observer/StockTicker` é o subject manual, com a flag `isolateFailures` no construtor — **os dois modos existem de propósito** e o cenário 3 executa os dois lado a lado para comparar. Não unificar.

`Events/EventBasedTicker` tem `Publish` (com `?.Invoke`) e `PublishIsolated` (percorrendo `GetInvocationList()`). Mesma razão: o par é a demonstração.

`StockTicker.Publish` itera sobre `_observers.ToArray()`. É necessário: um observador que se desinscreve durante a notificação modificaria a lista em uso.

## Pontos de atenção

- TFM `net10.0`. Pacote: `Microsoft.Extensions.Logging.Console` 10.0.12.
- **O cenário 5 depende de GC e é o único com resultado não garantido por contrato.** `AttachAndForget` cria o `HeavyObserver` em um método separado justamente para que não sobre referência forte no frame do chamador; depois de `GC.Collect()` o observador vazado aparece como vivo e o desinscrito como coletado. Isso é confiável em Debug nesta máquina e foi verificado, mas **não é garantia da linguagem** — inlining ou outro runtime podem mudar o resultado. Se o cenário passar a imprimir `False/False` ou `True/True`, é o GC, não uma regressão do padrão. Não transformar em teste automatizado.
- O `HeavyObserver` aloca 512 KB só para o vazamento ter peso. Não é usado para mais nada.
- No cenário 3, a **ordem de inscrição importa**: o `FaultyObserver` entra antes da auditoria, por isso a auditoria recebe 0 sem isolamento. Inverter a ordem faria o cenário não mostrar nada.
- O cenário 4 chama `-=` com um lambda de corpo idêntico ao do `+=`. Isso **não** remove nada, e é o ponto. Um analisador ou revisor desatento pode "corrigir" guardando o handler em variável — o que destruiria a demonstração.
- Os cenários são independentes entre si (cada um cria o próprio ticker), diferente de [CommandUndoRedoDemo](../CommandUndoRedoDemo/CLAUDE.md), onde o estado é compartilhado. Aqui dá para reordenar sem quebrar.
- `Program.cs` tem `await Task.Delay(200)` antes da linha final, pela fila do provider de console.
- **Fronteira com os vizinhos**: `DesignPattern/Behavioral` tem implementações introdutórias de padrões comportamentais. Aqui o foco é Observer com a comparação contra `event` e os modos de falha — não duplicar o básico. Notificação assíncrona com backpressure é assunto de `ChannelProducerConsumer` (trilha 02).
