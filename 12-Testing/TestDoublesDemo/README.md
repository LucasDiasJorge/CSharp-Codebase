# TestDoublesDemo

Biblioteca de domínio e suíte xUnit que diferencia dummy, stub, spy, mock e fake por exemplos, e mostra o custo real de escolher entre verificação de estado e de comportamento.

## Visão geral

Os cinco nomes costumam ser usados como sinônimos de "mock". Eles não são, e a diferença fica visível quando cada um é escrito à mão sobre o mesmo serviço:

- **Dummy** preenche um parâmetro que o caminho testado não usa. Aqui ele **lança exceção** se for chamado — vira documentação executável da premissa.
- **Stub** devolve resposta pronta para controlar o caminho. É como se testa "sem estoque" sem ter estoque.
- **Spy** grava as chamadas e deixa a verificação para o `Assert`.
- **Mock** recebe a expectativa **antes** da execução e falha por conta própria. A verificação mora dentro do dublê.
- **Fake** é implementação de verdade, simplificada: o repositório em memória guarda e devolve, e o teste lê de volta o que gravou.

A segunda metade do exemplo é a parte que costuma custar caro em projeto real. O checkout tem **duas implementações com o mesmo resultado de negócio**: uma notifica um item por vez, a outra agrupa tudo em uma mensagem. Pedido confirmado, pedido gravado, cliente avisado sobre todos os itens — idêntico nas duas.

A verificação de **estado** passa para as duas. O teste escrito sobre **contagem de chamadas** falha na segunda, com a mensagem `mock esperava 2 envio(s), recebeu 1` — mesmo sem nenhum defeito. É o preço de testar o *como* em vez do *quê*, e a suíte registra isso em um teste verde que documenta a fragilidade.

## Conceitos abordados

- Os cinco tipos de dublê de teste, na taxonomia de Meszaros.
- Dummy que falha ao ser usado como asserção de premissa.
- Stub controlando o caminho de execução por consulta.
- Spy (grava e deixa verificar) contra mock (cobra a expectativa).
- Fake com comportamento real e legível de volta.
- Verificação de estado contra verificação de comportamento.
- Acoplamento entre teste e implementação como custo mensurável.
- O mesmo conceito com biblioteca (`NSubstitute`) em vez de dublê escrito à mão.

## Objetivos de aprendizagem

- Nomear corretamente cada dublê e justificar a escolha pelo papel do colaborador.
- Decidir entre estado e comportamento a partir do que o colaborador devolve.
- Reconhecer, antes do code review, o teste que vai quebrar na próxima refatoração.
- Usar `NSubstitute` sabendo qual papel está sendo exercido em cada linha.

## Estrutura do projeto

```text
TestDoublesDemo/
|-- Abstractions/
|   `-- Collaborators.cs
|-- Checkout/
|   `-- OrderCheckout.cs
|-- Domain/
|   |-- CheckoutResult.cs
|   `-- Order.cs
|-- TestDoublesDemo.Tests/
|   |-- Doubles/
|   |   `-- Doubles.cs
|   |-- StateVersusBehaviorTests.cs
|   |-- TestDoubleKindsTests.cs
|   `-- TestDoublesDemo.Tests.csproj
|-- TestDoublesDemo.csproj
`-- README.md
```

## Como executar

```bash
dotnet test 12-Testing/TestDoublesDemo/TestDoublesDemo.Tests/TestDoublesDemo.Tests.csproj
```

Um teste específico:

```bash
dotnet test 12-Testing/TestDoublesDemo/TestDoublesDemo.Tests/TestDoublesDemo.Tests.csproj --filter "FullyQualifiedName~MockSobreContagemDeChamadas"
```

Para validar apenas a compilação da biblioteca:

```bash
dotnet build 12-Testing/TestDoublesDemo/TestDoublesDemo.csproj
```

Não exige serviço externo. São 10 testes, e todos passam.

## Boas práticas e pontos de atenção

- Escolha o dublê pelo papel do colaborador: consulta pede stub, comando pede spy ou mock, estado pede fake, parâmetro não usado pede dummy.
- Faça o dummy **falhar** se for chamado. Um dummy silencioso esconde que a premissa do teste mudou.
- Prefira verificação de estado quando existir estado observável. Ela sobrevive a refatoração; verificação de chamada não.
- Use verificação de comportamento quando não há o que observar — `INotificationSender` não devolve nada, e a única evidência de que o cliente foi avisado é a chamada.
- Verifique o **efeito**, não a contagem. `Assert.Contains(spy.Sent, m => m.Body.Contains("TECLADO"))` continua valendo depois do agrupamento; `Assert.Equal(2, spy.Sent.Count)` não.
- Limite o número de dublês por teste. Teste que precisa de quatro mocks configurados está dizendo que a unidade tem colaboradores demais.
- Fake é código, e código tem bug. Um fake que divirja do comportamento real produz suíte verde com produção quebrada — vale testar o fake contra o componente real em um teste de contrato.
- Biblioteca de mock não dispensa entender os papéis: `Returns(...)` é stub, `Received(...)` é mock. A mesma API faz as duas coisas, e chamar tudo de "mock" é o que produz suíte frágil.
- Dublê para tipo concreto (classe selada, método não virtual) não existe sem interface. Se o teste exige biblioteca de interceptação para funcionar, o problema costuma estar no desenho, não no teste.

## Conteúdo complementar

**Os cinco dublês, resumidos**:

| Dublê | Responde consulta? | Grava chamadas? | Tem expectativa própria? | Tem comportamento real? |
|---|---|---|---|---|
| Dummy | não | não | não | não |
| Stub | **sim** | não | não | não |
| Spy | às vezes | **sim** | não | não |
| Mock | às vezes | sim | **sim** | não |
| Fake | sim | não | não | **sim** |

O que separa spy de mock não é o que eles fazem durante a execução — é **quem cobra**. O spy grava e o `Assert` decide depois; o mock recebe a expectativa antes e falha sozinho.

**A refatoração que quebra o teste sem quebrar nada**:

```text
PerItemOrderCheckout  -> 2 envios: "item confirmado: TECLADO x1", "item confirmado: MOUSE x3"
BatchedOrderCheckout  -> 1 envio:  "pedido confirmado: TECLADO x1, MOUSE x3"
```

| Verificação | `PerItem` | `Batched` |
|---|---|---|
| Resultado confirmado | passa | passa |
| Pedido gravado no repositório | passa | passa |
| Cliente avisado sobre cada item | passa | passa |
| **Exatamente 2 chamadas ao notificador** | passa | **falha** |

```text
mock esperava 2 envio(s), recebeu 1
```

Nenhum requisito mudou. O que mudou foi o caminho — e só o teste acoplado ao caminho percebeu.

**O mesmo com `NSubstitute`**, para ligar os conceitos à ferramenta:

```csharp
inventory.HasStock(Arg.Any<string>(), Arg.Any<int>()).Returns(true);   // papel de STUB
notifications.Received(2).Send(CustomerEmail, Arg.Any<string>());      // papel de MOCK
```

Relação com os vizinhos: `OrderRuleConsole` cobre regras de negócio testadas de forma direta. `PropertyBasedTestingDemo` ataca o outro lado do mesmo problema — o que verificar, em vez de com o que substituir.

## Referências e documentação complementar

- https://martinfowler.com/bliki/TestDouble.html
- https://martinfowler.com/articles/mocksArentStubs.html
- https://nsubstitute.github.io/help/getting-started/
- https://learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test
