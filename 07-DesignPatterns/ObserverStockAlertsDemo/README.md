# ObserverStockAlertsDemo

Console que implementa o padrão Observer à mão, mostra o equivalente com `event` do C# e demonstra as quatro armadilhas que o padrão traz em código real.

## Visão geral

O Observer resolve um problema simples de enunciar: um objeto muda e vários outros precisam saber. Escrito à mão, são três peças — o subject com uma lista de inscritos, o contrato do observador e o método que percorre a lista avisando todo mundo.

Em C#, esse padrão já vem na linguagem. Um `event` é a lista de inscritos, `+=` é o Attach e `-=` é o Detach. Escrever a versão manual raramente se justifica, mas entendê-la explica o que o `event` faz e, principalmente, o que ele **não** faz.

A diferença que mais importa aparece quando um assinante falha. Com `?.Invoke`, a exceção do primeiro handler que quebra interrompe a invocação e os assinantes seguintes simplesmente não são avisados — sem erro visível para eles. Para isolar falhas é preciso percorrer `GetInvocationList()` à mão, coisa que o operador não oferece.

A armadilha mais cara é outra: o subject guarda referência forte para cada observador. Um observador que não se desinscreve nunca é coletado, e continua recebendo notificações que já não fazem sentido. É o *lapsed listener*, e o exemplo o mede — depois de forçar coleta, o observador vazado continua vivo e o que se desinscreveu não.

## Conceitos abordados

- Observer à mão: subject, contrato do observador, notificação.
- `event` do C# como o mesmo padrão embutido na linguagem.
- Falha de um assinante interrompendo a notificação dos demais.
- Isolamento de falhas com `GetInvocationList()`.
- Lapsed listener: vazamento por falta de desinscrição.
- Referência forte do subject e seu efeito sobre a coleta de lixo.
- Impossibilidade de remover um lambda anônimo não guardado.
- Iteração sobre cópia da lista para permitir desinscrição durante a notificação.

## Objetivos de aprendizagem

- Reconhecer que `event` é o Observer, e saber quando a versão manual compensa.
- Decidir, no subject, o que acontece quando um observador falha.
- Evitar o vazamento clássico de assinaturas que nunca são removidas.
- Entender por que guardar o delegate em variável é condição para poder removê-lo.

## Estrutura do projeto

```text
ObserverStockAlertsDemo/
|-- Demo/
|   `-- ObserverDemoRunner.cs
|-- Events/
|   `-- EventBasedTicker.cs
|-- Observer/
|   |-- AlertObservers.cs
|   |-- IStockObserver.cs
|   |-- StockQuote.cs
|   `-- StockTicker.cs
|-- ObserverStockAlertsDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/ObserverStockAlertsDemo/ObserverStockAlertsDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 07-DesignPatterns/ObserverStockAlertsDemo/ObserverStockAlertsDemo.csproj
```

Não exige serviço externo. Roda os cinco cenários e termina.

## Boas práticas e pontos de atenção

- Prefira `event` à implementação manual. A versão à mão só compensa quando o subject precisa de controle que o `event` não dá: ordem explícita, isolamento de falhas, prioridade entre observadores, ou inscrição com referência fraca.
- Decida o que fazer quando um assinante falha, e decida no subject. Com `?.Invoke`, a primeira exceção interrompe o resto da lista. Se cada assinante é independente, percorra `GetInvocationList()` e trate a falha de cada um.
- Desinscreva sempre. O subject guarda referência forte; enquanto ele viver, o observador inscrito não é coletado — e continua sendo notificado. `IDisposable` no observador, com `-=` no `Dispose`, é o caminho mais comum.
- Guarde o delegate em variável se pretende removê-lo. Dois lambdas com o mesmo corpo são objetos diferentes: `-=` não encontra nada, não remove nada e **não avisa**.
- Itere sobre uma cópia da lista ao notificar. Um observador que se desinscreve durante a própria notificação modificaria a coleção em uso e derrubaria o laço.
- Não presuma ordem de notificação como parte do contrato. `event` invoca na ordem de inscrição, mas depender disso acopla os assinantes entre si — se a ordem importa, o caso provavelmente não é Observer.
- Notificação síncrona significa que o subject espera por todos. Um observador lento atrasa os demais e quem publicou; se isso for problema, o caminho é fila, não Observer.
- Considere `IObservable<T>`/`IObserver<T>` quando precisar compor fluxos (filtrar, agrupar, aplicar janela). Para "avise quando mudar", `event` basta.

## Conteúdo complementar

Correspondência entre a versão manual e o `event`:

| Padrão à mão | C# |
|---|---|
| `List<IStockObserver>` no subject | Campo de backing do `event` |
| `Attach(observer)` | `evento += handler` |
| `Detach(observer)` | `evento -= handler` |
| `foreach` notificando | `evento?.Invoke(...)` |
| `IStockObserver.OnQuote` | Assinatura do `EventHandler<T>` |

Resultados observados nos cenários:

**3. Falha de um observador**

```text
SEM isolamento: a auditoria recebeu 0 cotacoes
COM isolamento: a auditoria recebeu 1 cotacao
```

O observador defeituoso estava inscrito antes da auditoria. Sem isolamento, a exceção dele interrompeu o laço e a auditoria nunca foi chamada.

**4. Remoção de lambda anônimo**

```text
Inscrito com lambda anonimo. Assinantes: 1.
Depois do -= com outro lambda de corpo identico: 1 assinante(s).
```

Nada foi removido e nenhum erro apareceu. O código parece correto e não funciona.

**5. Lapsed listener**

```text
Apos coleta: o observador que NAO se desinscreveu continua vivo? True
              o que se desinscreveu?                            False
O ticker com vazamento ainda lista 1 observador; o outro, 0.
```

Os dois observadores saíram de escopo. Só o que continuou inscrito sobreviveu à coleta — mantido vivo pela lista do subject, com o payload inteiro junto.

Quando escolher cada abordagem:

| Necessidade | Escolha |
|---|---|
| Avisar que algo mudou, dentro do processo | `event` |
| Isolar falha de assinante, controlar ordem ou prioridade | Observer à mão |
| Compor, filtrar, agrupar ou aplicar janela sobre o fluxo | `IObservable<T>` / Rx |
| Assinante lento, ou em outro processo | Fila de mensagens, não Observer |

Relação com os vizinhos da trilha: `DesignPattern/Behavioral` reúne implementações introdutórias de padrões comportamentais; este projeto isola Observer e trata do que o básico não cobre — a comparação com `event` e os modos de falha. `ChannelProducerConsumer` (trilha 02) é a resposta quando a notificação precisa ser assíncrona e com backpressure.

## Referências e documentação complementar

- https://refactoring.guru/design-patterns/observer
- https://learn.microsoft.com/dotnet/csharp/programming-guide/events/
- https://learn.microsoft.com/dotnet/standard/events/observer-design-pattern
- https://en.wikipedia.org/wiki/Lapsed_listener_problem
