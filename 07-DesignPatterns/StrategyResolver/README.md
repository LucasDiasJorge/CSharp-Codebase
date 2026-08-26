# StrategyResolver

Evolução do padrão Strategy em que cada estratégia declara sozinha quando se aplica, eliminando o `switch` que normalmente escolhe o algoritmo. Baseado no artigo [The Strategy Pattern — Advanced Resolver Pattern](https://alchemy86.github.io/2019/06/18/The-strategy-pattern-Advanced-Resolver-Pattern/).

## Visão geral

No Strategy clássico o chamador precisa saber qual implementação usar: alguém, em algum lugar, traduz um dado (`"OVEN"`, `"GRILL"`) para uma classe concreta — quase sempre num `switch` dentro do próprio serviço que orquestra. O algoritmo fica isolado, mas a decisão sobre ele não: cada caso novo obriga a editar esse `switch`.

O Advanced Resolver Pattern move a decisão para dentro da estratégia. O contrato ganha um método `AppliesTo`, cada resolver responde se sabe tratar aquele dado, e o orquestrador apenas pergunta à coleção injetada quem aceita o trabalho — com um fallback explícito para o que ninguém aceitar. O exemplo roda três etapas em sequência para tornar a diferença observável: primeiro a versão com `switch`, depois a versão com resolvers deixando um método sem dono, e por fim a mesma execução com um resolver a mais registrado. Entre a segunda e a terceira etapa nenhuma linha do orquestrador muda — só o composition root.

## Conceitos abordados

- Strategy Pattern e sua limitação quando a seleção fica no chamador.
- Resolver auto-descritivo: `AppliesTo` como critério de aplicabilidade dentro da própria estratégia.
- Injeção de coleção: registrar várias implementações do mesmo contrato e recebê-las como `IEnumerable<T>`.
- Fallback explícito (`DefaultResolver`) para discriminadores desconhecidos.
- Open/Closed Principle na prática: estender o comportamento sem tocar no código que orquestra.
- Agrupamento por discriminador e execução paralela dos lotes com `Task.WhenAll`.

## Objetivos de aprendizagem

- Identificar o `switch` de seleção de estratégia como ponto de violação do Open/Closed Principle.
- Escrever um contrato de resolver que carregue o próprio critério de seleção.
- Registrar múltiplas implementações do mesmo serviço no `Microsoft.Extensions.DependencyInjection` e consumi-las como coleção.
- Decidir entre fallback por ordem de registro e fallback injetado à parte, entendendo o risco de cada um.
- Perceber que a ordem de registro vira regra de precedência quando dois resolvers aceitam o mesmo dado.

## Estrutura do projeto

```text
StrategyResolver/
|-- Domain/
|   |-- CookingMethods.cs
|   `-- FoodDish.cs
|-- Legacy/
|   `-- SwitchCookingService.cs
|-- Resolvers/
|   |-- IFoodCookingResolver.cs
|   |-- OvenResolver.cs
|   |-- GrillResolver.cs
|   |-- SousVideResolver.cs
|   `-- DefaultResolver.cs
|-- Services/
|   |-- IDishResolutionService.cs
|   `-- DishResolutionService.cs
|-- CookingModule.cs
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

Não há dependência de serviço externo: o preparo dos pratos é simulado com `Task.Delay`.

## Boas práticas e pontos de atenção

- **`AppliesTo` deve ser barato e sem efeito colateral.** Ele é chamado para cada candidato até o primeiro que aceitar; consulta a banco ou I/O nesse método transforma a seleção em gargalo.
- **A ordem de registro é a regra de precedência.** `FirstOrDefault` para no primeiro resolver que aceitar, então dois resolvers que respondam `true` para o mesmo discriminador tornam o comportamento dependente da ordem das linhas em `CookingModule`. Se isso for possível no seu domínio, prefira um critério explícito de prioridade a confiar no registro.
- **O `DefaultResolver` fica fora da coleção.** Como seu `AppliesTo` sempre retorna `true`, incluí-lo entre os candidatos faria dele o único escolhido caso fosse registrado antes dos demais. Injetá-lo pelo tipo concreto torna o fallback independente da ordem.
- **O fallback existe para não perder trabalho em silêncio.** Ele registra um `LogWarning` com o discriminador desconhecido, em vez de lançar exceção ou ignorar o item.
- **O `switch` em `Legacy/SwitchCookingService.cs` é contraste didático**, não alternativa recomendada. Está no projeto para que a diferença apareça na saída do console.
- O projeto usa `net10.0`. Os logs saem pelo `ILogger` com `AddSimpleConsole`, então a ordem entre lotes paralelos pode variar entre execuções.

## Conteúdo complementar

### Strategy clássico e Resolver lado a lado

| Aspecto | Strategy clássico | Advanced Resolver |
|---|---|---|
| Quem escolhe a implementação | O chamador ou um `switch`/factory | O próprio resolver, via `AppliesTo` |
| Adicionar um caso novo | Editar o `switch` e recompilar o orquestrador | Registrar mais uma linha no composition root |
| Conhecimento do orquestrador | Todas as implementações concretas | Apenas o contrato e o fallback |
| Dado desconhecido | Caso `default` ou exceção | `DefaultResolver` explícito |
| Onde erra com facilidade | `switch` esquecido em outro ponto do código | Dois resolvers aceitando o mesmo dado |

Em `07-DesignPatterns/StrategyIntegration` está a forma clássica do mesmo padrão, com contexto e `SetStrategy` — vale ler os dois em sequência.

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

Aqui o mesmo resultado sai do próprio container: registrar `IFoodCookingResolver` várias vezes faz o `Microsoft.Extensions.DependencyInjection` entregar todas as implementações quando o construtor pede `IEnumerable<IFoodCookingResolver>`. A vantagem prática é que cada resolver passa a receber suas próprias dependências pelo container, sem a fábrica precisar saber montá-las.

Vale notar que o trecho de seleção publicado no artigo não compila como está:

```csharp
// artigo - sem parenteses, o ?? tenta comparar um resolver com uma Task
_controlDataResolvers.FirstOrDefault(x => x.AppliesTo(item.Key))
    ?? new DefaultResolver().CookFoodAsync(conn, item.ToList())
```

O acesso a membro tem precedência maior que o `??`, então o compilador lê `resolver ?? (new DefaultResolver().CookFoodAsync(...))` — e os dois lados têm tipos incompatíveis. Em `DishResolutionService.SelectResolver` a escolha é resolvida numa variável antes da chamada, o que elimina a ambiguidade e ainda permite logar qual resolver venceu.

### Exercícios sugeridos

1. Crie um `AirFryerResolver` e registre-o em `CookingModule`. Confirme que `DishResolutionService` não precisou de nenhuma alteração.
2. Faça dois resolvers responderem `true` para `CookingMethods.Oven` e observe qual vence; depois inverta as linhas de registro.
3. Troque o `FirstOrDefault` por `Where(...)` e execute todos os resolvers aplicáveis — decida se o seu domínio quer seleção única ou difusão.
4. Substitua o `AppliesTo(string)` por uma sobrecarga que receba o `FoodDish` inteiro e permita critérios compostos, como método de cocção somado ao tempo de preparo.

## Referências e documentação complementar

- [The Strategy Pattern — Advanced Resolver Pattern](https://alchemy86.github.io/2019/06/18/The-strategy-pattern-Advanced-Resolver-Pattern/) — artigo que originou este exemplo.
- [Injeção de dependência no .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection) — registro de múltiplas implementações do mesmo contrato.
- [Strategy Pattern (Refactoring Guru)](https://refactoring.guru/design-patterns/strategy)
- `07-DesignPatterns/StrategyIntegration` — Strategy clássico com contexto explícito.
- `07-DesignPatterns/DesignPattern/Behavioral/Strategy` — forma canônica do padrão GoF.
