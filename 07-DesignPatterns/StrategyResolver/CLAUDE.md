# CLAUDE.md — StrategyResolver

Console: Advanced Resolver Pattern — Strategy em que cada implementação declara sua própria aplicabilidade via `AppliesTo`, sem `switch` no orquestrador. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/StrategyResolver/StrategyResolver.csproj
dotnet run --project 07-DesignPatterns/StrategyResolver/StrategyResolver.csproj
```

## Estrutura interna

- `Resolvers/IFoodCookingResolver.cs` — **o contrato onde o padrão vive**. `AppliesTo(string)` é o que separa este exemplo do Strategy clássico: a decisão de seleção pertence à estratégia, não ao chamador.
- `Resolvers/{Oven,Grill,SousVide}Resolver.cs` — implementações independentes; `GrillResolver` executa em paralelo e `OvenResolver` sequencial de propósito, para mostrar que cada estratégia mantém suas decisões internas.
- `Resolvers/DefaultResolver.cs` — fallback com `AppliesTo` sempre `true`. **Fica fora da coleção `IEnumerable<IFoodCookingResolver>`** e é injetado pelo tipo concreto; se entrasse na coleção, engoliria os demais quando registrado primeiro.
- `Services/DishResolutionService.cs` — orquestrador que agrupa por método de cocção, seleciona com `FirstOrDefault(AppliesTo)` e dispara os lotes com `Task.WhenAll`. É o arquivo que **não muda** quando um resolver novo entra.
- `CookingModule.cs` — composition root. O parâmetro `withSousVideResolver` liga/desliga um resolver para o `Program` demonstrar o Open/Closed Principle em tempo de execução.
- `Legacy/SwitchCookingService.cs` — a versão com `switch`, mantida só como contraste didático na etapa 1 da saída.
- `Program.cs` — top-level statements em três etapas: `switch` → resolvers sem SousVide (cai no fallback) → resolvers com SousVide.

## Pontos de atenção

- TFM **`net10.0`**, não o `net9.0` padrão do repositório: a máquina tem apenas os runtimes 8.0 e 10.0 instalados, então um build `net9.0` compila mas falha em `dotnet run`. Não "padronizar" para net9.0 sem instalar o runtime correspondente.
- Pacotes `Microsoft.Extensions.DependencyInjection` e `Microsoft.Extensions.Logging.Console` 10.0.0. Sem serviço externo — o preparo é `Task.Delay`, roda offline.
- A ordem das linhas `AddSingleton<IFoodCookingResolver, ...>` em `CookingModule` é a regra de precedência da seleção. Ao adicionar resolver novo, verificar se o `AppliesTo` dele não colide com um já registrado.
- A saída mistura logs de lotes paralelos; a ordem entre grupos varia entre execuções e isso não é defeito.
- Par de estudo: `StrategyIntegration` (mesma pasta) tem o Strategy clássico com `SetStrategy`; `DesignPattern/Behavioral/Strategy` tem a forma GoF pura; `PortsAndAdapters/example` leva seleção dinâmica para uma API.
