# CLAUDE.md — PersistencePatterns

Console: Repository, Unit of Work e Identity Map implementados lado a lado. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 08-ArchitecturalPatterns/PersistencePatterns/PersistencePatterns.csproj
dotnet run --project 08-ArchitecturalPatterns/PersistencePatterns/PersistencePatterns.csproj
```

## Estrutura interna

- `Core/` — `IEntity`, `IRepository`, `IUnitOfWork`: abstrações comuns.
- `Examples/Repository/` — abstração de acesso a dados sobre `Product`.
- `Examples/UnitOfWork/` — `OrderUnitOfWork` coordena repositórios de `Order` e `Payment` num commit único.
- `Examples/IdentityMap/` — `CustomerRepositoryWithIdentityMap` + `IdentityMap`: garante **uma única instância em memória por identidade** dentro da mesma sessão. Sem isso, duas leituras do mesmo cliente produzem dois objetos que divergem ao serem alterados.

Cada padrão tem sua tríade `Entities/`, `Interfaces/`, `Implementations/` — a repetição é intencional, mantém os exemplos independentes.

## Pontos de atenção

- TFM `net8.0`, **tudo em memória**, sem EF Core nem banco. É deliberado: esses padrões costumam ser vistos só embutidos no EF Core, e aqui aparecem explícitos.
- Para as mesmas ideias com persistência real, ver `03-WebAPIs/TransactionalOrderApi`.
