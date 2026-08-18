# CLAUDE.md — MysqlExample

Web API com EF Core e MySQL, incluindo migrações versionadas. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name mysql -e MYSQL_ROOT_PASSWORD=root -p 3306:3306 mysql

dotnet run --project 09-Data/Data/MysqlExample/MysqlExample.csproj

# migrações (exigem dotnet-ef instalado)
dotnet ef database update --project 09-Data/Data/MysqlExample/MysqlExample.csproj
```

## Estrutura interna

- `src/DbContext.cs` — contexto EF Core (a classe se chama `MeuDbContext`, conforme o snapshot).
- `Migrations/` — migração `Inicial` versionada com `MeuDbContextModelSnapshot`. Alterações no modelo exigem **nova** migração; não edite as existentes nem o snapshot à mão.
- `WeatherForecast.cs` — entidade de exemplo, herdada do template.

## Pontos de atenção

- **Exige MySQL ativo**; connection string em `appsettings.json`.
- TFM `net9.0`. `MySql.EntityFrameworkCore` 9.0.3.
- É o exemplo "EF Core + MySQL" da trilha; compare com `DapperExample` (SQL à mão, mesmo banco) e `Postgres` (EF Core, outro banco).
