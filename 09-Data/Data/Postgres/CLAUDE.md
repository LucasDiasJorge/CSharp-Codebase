# CLAUDE.md — Postgres

Console com EF Core e PostgreSQL, modelando usuários e papéis. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres

dotnet run --project 09-Data/Data/Postgres/Postgres.csproj
```

## Estrutura interna

- `AppDbContext.cs` — contexto e configuração do relacionamento.
- `Models/Users.cs` e `Models/Roles.cs` — o par que demonstra relacionamento entre entidades.
- `Migrations/` — `InitialCreate` versionada com snapshot; mudanças de modelo pedem nova migração.

É **console**, não Web API: o foco fica no acesso a dados, sem camada HTTP no caminho.

## Pontos de atenção

- **Exige PostgreSQL ativo**; connection string no código.
- TFM `net9.0`. `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.4.
- Outros projetos do repositório também usam Postgres (`02-AsyncAndConcurrency/TaskManagement`, `03-WebAPIs/WebApplication/MyAPI`) — o mesmo container serve para os três.
