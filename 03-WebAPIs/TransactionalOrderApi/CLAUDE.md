# CLAUDE.md — TransactionalOrderApi

API em camadas que demonstra onde o controle transacional deve viver num fluxo de pedidos. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/TransactionalOrderApi/TransactionalOrderApi.csproj
dotnet run --project 03-WebAPIs/TransactionalOrderApi/TransactionalOrderApi.csproj
```

## Estrutura interna

Projeto em camadas explícitas (20 arquivos) — o mais estruturado da trilha:

- `Domain/Entities/` — `Customer`, `Order`, `OrderItem`.
- `Infrastructure/Persistence/` — `AppDbContext` + `Configurations/` (Fluent API por entidade, uma classe cada).
- `Infrastructure/Repositories/` — repositórios por agregado, atrás de interfaces.
- `Application/Services/` — `OrderService` orquestra; `ITransactionRunner`/`EfCoreTransactionRunner` **isolam a transação atrás de uma abstração**, de modo que a camada de aplicação decide o limite transacional sem depender de EF Core.
- `Api/Controllers/OrdersController.cs` — fino, só traduz HTTP.
- `Templates/GenericUnitOfWork.cs` — alternativa de referência, não o caminho principal.

A tese do exemplo: a transação pertence ao caso de uso (application), não ao repositório nem ao controller.

## Pontos de atenção

- TFM `net9.0`. EF Core 9.0.0 + **SQLite** (arquivo local, sem servidor externo).
- Não há pasta `Migrations/` embora `EntityFrameworkCore.Design` esteja referenciado — o schema vem de criação em runtime. Adicionar migrações muda esse contrato.
