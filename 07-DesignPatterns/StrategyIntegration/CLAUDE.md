# CLAUDE.md — StrategyIntegration

Console: Strategy aplicado a integrações com provedores distintos. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/StrategyIntegration/StrategyIntegration.csproj
dotnet run --project 07-DesignPatterns/StrategyIntegration/StrategyIntegration.csproj
```

## Estrutura interna

- `Interfaces/IIntegrationStrategy.cs` — contrato comum às integrações.
- `IntegrationClasses/FirstIntegration.cs` / `SecondIntegration.cs` — provedores com formatos e comportamentos diferentes.
- `Response.cs` — **o tipo de retorno normalizado**. É a peça que faz a estratégia valer: sem uma resposta comum, o chamador voltaria a precisar saber qual provedor respondeu.
- `IntegrationStrategy.cs` — contexto que seleciona e executa.

## Pontos de atenção

- TFM `net9.0`, **sem chamadas HTTP reais** — os provedores são simulados, então roda offline.
- Versão aplicada do padrão que `DesignPattern/Behavioral/Strategy` apresenta em forma pura; `PortsAndAdapters/example` leva a mesma ideia para seleção em runtime numa API.
