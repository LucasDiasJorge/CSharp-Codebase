# CLAUDE.md — Factory

Console: criação de veículos sem que o chamador conheça a classe concreta. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Creational/Factory/Factory.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Creational/Factory/Factory.csproj
```

## Estrutura interna

- `IVeiculo.cs` — abstração devolvida ao chamador.
- `Veiculos.cs` — as implementações concretas.
- `TipoVeiculo.cs` — enum que seleciona.
- `VeiculoFactory.cs` — **o único ponto do código que conhece os tipos concretos**. Essa concentração é o objetivo: um lugar só para mudar quando um tipo novo entra.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Ao adicionar um veículo, o enum e o `switch` da factory mudam juntos — se essa dupla incomodar, o passo seguinte é registro por dicionário, que é justamente o que `Creational/Registry` demonstra.
