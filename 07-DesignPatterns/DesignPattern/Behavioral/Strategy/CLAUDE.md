# CLAUDE.md — Strategy

Console: processamento de pagamentos com algoritmos intercambiáveis. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Behavioral/Strategy/Strategy.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/Strategy/Strategy.csproj
```

## Estrutura interna

Arquivo único (`Program.cs`): uma interface de pagamento, várias implementações (cartão, boleto, pix) e um contexto que recebe a estratégia e a executa sem conhecer a concreta.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Há **três** exemplos de Strategy no repositório, em níveis diferentes: este (didático puro), `07-DesignPatterns/StrategyIntegration` (aplicado a integrações) e `07-DesignPatterns/PortsAndAdapters/example` (seleção dinâmica via DI numa API). Ao editar, preserve a progressão em vez de duplicar conteúdo.
