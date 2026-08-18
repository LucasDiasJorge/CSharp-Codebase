# CLAUDE.md — VerticalSliceMinimalApi

Minimal API organizada por **vertical slice**: cada funcionalidade agrupa endpoint, contratos e handler. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/VerticalSliceMinimalApi/VerticalSliceMinimalApi.csproj
dotnet run --project 03-WebAPIs/VerticalSliceMinimalApi/VerticalSliceMinimalApi.csproj
```

## Estrutura interna

`Features/Orders/{Create,GetById,List}/` — cada pasta é uma fatia autocontida com seu `*Endpoint`, `*Handler`, `*Request` e `*Response`. Nada é compartilhado entre fatias além de `Domain/Order.cs` e do repositório.

- `Infrastructure/Orders/InMemoryOrderRepository.cs` (+ `IOrderRepository`) — persistência em memória.
- `Program.cs` — registra os endpoints de cada fatia.

**A regra de ouro aqui**: ao adicionar uma funcionalidade, crie uma nova pasta em `Features/Orders/` com o conjunto completo. Não extraia um `OrdersController` nem uma pasta `DTOs/` compartilhada — isso reintroduz exatamente o acoplamento horizontal que o padrão existe para eliminar. Contraste direto com `TransactionalOrderApi` (mesmo domínio, organização em camadas).

## Pontos de atenção

- TFM `net9.0`, **sem pacotes externos** — não usa MediatR; os handlers são chamados diretamente.
- Dados em memória: somem a cada restart.
