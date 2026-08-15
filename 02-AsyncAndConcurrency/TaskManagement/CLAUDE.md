# CLAUDE.md — TaskManagement

Gerenciador de tarefas com EF Core e PostgreSQL. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 02-AsyncAndConcurrency/TaskManagement/TaskManagement.csproj
dotnet run --project 02-AsyncAndConcurrency/TaskManagement/TaskManagement.csproj
```

## Estrutura interna

- `AppDbContext.cs` — contexto EF Core mapeando `Models/TaskModel.cs`.
- `Enums/TaskStatus.cs` e `Enums/TaskPriority.cs` — **atenção**: `TaskStatus` colide em nome com `System.Threading.Tasks.TaskStatus`, e `ImplicitUsings` traz esse namespace. Ambiguidade aqui se resolve por qualificação ou alias; não "simplifique" removendo a qualificação.
- `Migrations/` — migração `InitialCreate` versionada, com snapshot. Alterações no modelo exigem nova migração, não edição da existente.

## Pontos de atenção

- **Exige PostgreSQL ativo**; a connection string está no código/configuração do projeto. Sem o banco, a execução falha na conexão.

  ```bash
  docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres
  ```
- TFM `net9.0`. Pacotes: EF Core 9.0.2, `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.3.
- O `.csproj` referencia `System.Linq` 4.3.0 — pacote legado desnecessário em .NET moderno (LINQ está no runtime). Não replique.
