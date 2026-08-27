# StrategyResolver

Advanced Resolver Pattern aplicado a um processador de pagamentos: cada método de pagamento é uma estratégia que declara sozinha quando se aplica e com que precedência, eliminando o `switch` de seleção. Baseado no artigo [The Strategy Pattern — Advanced Resolver Pattern](https://alchemy86.github.io/2019/06/18/The-strategy-pattern-Advanced-Resolver-Pattern/).

## Visão geral

No Strategy clássico o chamador precisa saber qual implementação usar: alguém, em algum lugar, traduz um dado (`"PIX"`, `"CREDIT_CARD"`) para uma classe concreta — quase sempre num `switch` dentro do próprio serviço que orquestra. O algoritmo fica isolado, mas a decisão sobre ele não: cada método novo obriga a editar esse `switch`, e critérios compostos viram `if` aninhado dentro do `case`.

O Advanced Resolver Pattern move a decisão para dentro da estratégia. O contrato ganha `AppliesTo(PaymentRequest)` — recebendo o pedido inteiro, não só uma string — e `Priority`, que torna a precedência um dado explícito do resolver. O orquestrador apenas pergunta à coleção injetada quem assume cada pedido, com um fallback explícito para o que ninguém aceitar.

O exemplo roda três etapas em sequência para tornar a diferença observável: primeiro a versão com `switch`; depois os resolvers auto-registrados por varredura de assembly, com a carteira digital ainda inexistente e caindo no fallback; por fim a mesma execução com a classe presente no assembly. Entre a segunda e a terceira etapa nada muda — nem o orquestrador, nem sequer o registro de dependências.

## Conceitos abordados

- Strategy Pattern e sua limitação quando a seleção fica no chamador.
- Resolver auto-descritivo: `AppliesTo` recebendo o objeto de domínio, permitindo critério composto.
- `Priority` como contrato explícito de precedência entre resolvers que disputam o mesmo pedido.
- Auto-registro por varredura de assembly (`Assembly.GetTypes()` + `IsAssignableFrom`).
- Registro de módulo via extension method sobre `IServiceCollection`.
- Injeção de coleção: várias implementações do mesmo contrato entregues como `IEnumerable<T>`.
- Fallback explícito (`UnsupportedPaymentResolver`) para métodos desconhecidos.
- Open/Closed Principle na prática: estender o comportamento sem tocar no código que orquestra.

## Objetivos de aprendizagem

- Identificar o `switch` de seleção de estratégia como ponto de violação do Open/Closed Principle.
- Escrever um contrato de resolver que carregue o próprio critério de seleção e a própria precedência.
- Registrar múltiplas implementações do mesmo serviço por varredura de assembly e consumi-las como coleção.
- Entender por que o auto-registro **exige** uma prioridade explícita, em vez de confiar na ordem das linhas de `AddSingleton`.
- Decidir entre fallback por ordem de registro e fallback injetado à parte, entendendo o risco de cada um.
- Reconhecer quando dois resolvers aceitam o mesmo pedido e desambiguar o caso de forma intencional.

## Estrutura do projeto

```text
StrategyResolver/
|-- DependencyInjection/
|   `-- PaymentResolverRegistration.cs
|-- Domain/
|   |-- PaymentMethods.cs
|   `-- PaymentRequest.cs
|-- Legacy/
|   `-- SwitchPaymentService.cs
|-- Resolvers/
|   |-- IPaymentResolver.cs
|   |-- PixResolver.cs
|   |-- BoletoResolver.cs
|   |-- CreditCardResolver.cs
|   |-- CreditCardInstallmentResolver.cs
|   |-- DigitalWalletResolver.cs
|   `-- UnsupportedPaymentResolver.cs
|-- Services/
|   |-- IPaymentProcessingService.cs
|   `-- PaymentProcessingService.cs
|-- Program.cs
`-- StrategyResolver.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/StrategyResolver/StrategyResolver.csproj
```

Somente compilar:

```bash
dotnet build 07-DesignPatterns/StrategyResolver/StrategyResolver.csproj
```

Não há dependência de serviço externo nem chamada a adquirente real: a liquidação é simulada com `Task.Delay` e logs.

## Boas práticas e pontos de atenção

- **Com auto-registro, prioridade explícita deixa de ser opcional.** A ordem devolvida por `Assembly.GetTypes()` não é garantida pela especificação. Enquanto o registro era manual dava para (mal) confiar na sequência das linhas de `AddSingleton`; com scanning, a precedência precisa ser um dado do resolver — é o papel de `IPaymentResolver.Priority`, ordenado uma única vez no construtor de `PaymentProcessingService`.
- **Menor prioridade é avaliada primeiro**, na mesma convenção do `Order` de middlewares e filtros do ASP.NET Core. Resolvers específicos recebem valor menor que os genéricos: `CreditCardInstallmentResolver` (100) aceita um subconjunto do que `CreditCardResolver` (200) aceita, então precisa vir antes.
- **`OrderBy` é estável, e isso importa.** Resolvers com a mesma prioridade mantêm a ordem em que o scanning os devolveu — ou seja, prioridades empatadas voltam a ser indeterminadas. Empate só é seguro entre resolvers cujos `AppliesTo` são mutuamente exclusivos.
- **`AppliesTo` deve ser barato e sem efeito colateral.** Ele é chamado para os candidatos até o primeiro que aceitar; consulta a banco, antifraude ou I/O nesse método transforma a seleção em gargalo.
- **O `UnsupportedPaymentResolver` fica fora da coleção.** Como seu `AppliesTo` sempre retorna `true`, incluí-lo entre os candidatos o faria vencer qualquer disputa em que fosse avaliado primeiro. O scanning o exclui explicitamente e ele é injetado pelo tipo concreto, o que torna o fallback independente de ordem e de prioridade.
- **O agrupamento é pelo resolver vencedor, não pelo método de pagamento.** Com critério composto, dois pedidos `CREDIT_CARD` caem em resolvers diferentes; agrupar pela string do método quebraria o lote. A chave de agrupamento é a instância do resolver, o que depende de eles serem registrados como singleton.
- **O `switch` em `Legacy/SwitchPaymentService.cs` é contraste didático**, não alternativa recomendada. Está no projeto para que a diferença apareça na saída do console.
- O projeto usa `net10.0`. Os logs saem pelo `ILogger` com `AddSimpleConsole`, então a ordem entre lotes paralelos varia entre execuções.

## Conteúdo complementar

### Strategy clássico e Advanced Resolver lado a lado

| Aspecto | Strategy clássico | Advanced Resolver |
|---|---|---|
| Quem escolhe a implementação | O chamador ou um `switch`/factory | O próprio resolver, via `AppliesTo` |
| Critério de seleção | Uma chave simples, no rótulo do `case` | O objeto de domínio inteiro, com regra composta |
| Adicionar um caso novo | Editar o `switch` e recompilar o orquestrador | Criar a classe; o scanning a encontra |
| Precedência entre candidatos | Implícita na ordem do `switch` | Explícita em `Priority` |
| Conhecimento do orquestrador | Todas as implementações concretas | Apenas o contrato e o fallback |
| Dado desconhecido | Caso `default` ou exceção | Fallback dedicado e auditável |
| Onde erra com facilidade | `switch` esquecido em outro ponto do código | Prioridades empatadas entre resolvers que colidem |

Em `07-DesignPatterns/StrategyIntegration` está a forma clássica do mesmo padrão, com contexto e `SetStrategy` — vale ler os dois em sequência.

### O que a saída do console demonstra

Etapa 2 imprime a fila de avaliação antes de processar:

```text
Resolvers na ordem de avaliação (prioridade crescente):
  [ 100] BoletoResolver
  [ 100] CreditCardInstallmentResolver
  [ 100] PixResolver
  [ 200] CreditCardResolver
```

Dois pontos ficam visíveis no lote processado logo abaixo:

- `PED-1003` (cartão, 1x) vai para `CreditCardResolver`, mas `PED-1004` (cartão, 6x) vai para `CreditCardInstallmentResolver` — mesmo método de pagamento, resolvers diferentes, decidido por `Priority` somado ao critério composto.
- `PED-1005` (carteira digital) é recusado pelo `UnsupportedPaymentResolver` com `LogWarning`, em vez de estourar exceção. Na etapa 3, a mesma execução o liquida normalmente — sem nenhuma alteração em `PaymentProcessingService` ou em `PaymentResolverRegistration`.

### Do artigo para o código deste projeto

O artigo monta a coleção de resolvers à mão, com uma fábrica que faz `yield return` de cada implementação:

```csharp
private IEnumerable<IFoodCookingResolver> GetResolvers(IServiceProvider ctx)
{
    yield return new FishResolver(ctx.GetService<IConnectionFactory>());
    // ...
    yield return new DefaultResolver();
}
```

Aqui o mesmo resultado sai do container, por varredura do assembly em `AddPaymentResolvers`. São duas vantagens práticas: cada resolver recebe suas próprias dependências pelo container, sem a fábrica saber montá-las; e criar um método de pagamento novo deixa de exigir edição em qualquer arquivo existente.

Vale notar que o trecho de seleção publicado no artigo não compila como está:

```csharp
// artigo - sem parenteses, o ?? tenta comparar um resolver com uma Task
_controlDataResolvers.FirstOrDefault(x => x.AppliesTo(item.Key))
    ?? new DefaultResolver().CookFoodAsync(conn, item.ToList())
```

O acesso a membro tem precedência maior que o `??`, então o compilador lê `resolver ?? (new DefaultResolver().CookFoodAsync(...))` — e os dois lados têm tipos incompatíveis. Em `PaymentProcessingService.SelectResolver` a escolha é resolvida numa variável antes da chamada, o que elimina a ambiguidade e ainda permite logar qual resolver venceu.

### Exercícios sugeridos

1. Crie um `PixParceladoResolver` e não registre nada: confirme que o scanning o encontra e que `PaymentProcessingService` não precisou de alteração.
2. Dê prioridade 100 ao `CreditCardResolver`, empatando com o parcelado, e observe o comportamento ficar dependente da ordem do scanning.
3. Faça `ProcessAsync` devolver um `PaymentResult` tipado com sucesso, recusa e motivo, em vez de `Task`. Erro de negócio vira dado, não exceção.
4. Aplique Decorator sobre `IPaymentResolver` para retry com backoff e cronometragem, sem nenhum resolver concreto tomar conhecimento disso.
5. Escreva testes xUnit cobrindo o `AppliesTo` de cada resolver, a precedência entre os dois resolvers de cartão e o caminho de fallback.

## Referências e documentação complementar

- [The Strategy Pattern — Advanced Resolver Pattern](https://alchemy86.github.io/2019/06/18/The-strategy-pattern-Advanced-Resolver-Pattern/) — artigo que originou este exemplo.
- [Injeção de dependência no .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection) — registro de múltiplas implementações do mesmo contrato.
- [Strategy Pattern (Refactoring Guru)](https://refactoring.guru/design-patterns/strategy)
- `07-DesignPatterns/StrategyIntegration` — Strategy clássico com contexto explícito.
- `07-DesignPatterns/DesignPattern/Behavioral/Strategy` — forma canônica do padrão GoF.
