# CLAUDE.md — Adapter

Console: código legado consumido através de uma interface moderna. Regras globais em [CLAUDE.md](../../../../CLAUDE.md).

## Comandos

```bash
dotnet build 07-DesignPatterns/DesignPattern/Structural/Adapter/Adapter.csproj
dotnet run --project 07-DesignPatterns/DesignPattern/Structural/Adapter/Adapter.csproj
```

## Estrutura interna

- `Legacy/LegacyDatabase.cs` — a API antiga, **intocável** por definição do exercício.
- `Interfaces/IClientRepository.cs` — o contrato que a aplicação quer usar.
- `Adapters/ClientRepositoryAdapter.cs` — a tradução entre os dois.
- `Models/Client.cs` — modelo do lado moderno.

Ao estender, **não modifique `Legacy/`**: o valor do exemplo está justamente em adaptar o que não se pode alterar.

## Pontos de atenção

- TFM `net9.0`, sem dependências externas.
- Conceitualmente vizinho de `PortsAndAdapters/example`, que eleva a mesma ideia a estilo arquitetural.
