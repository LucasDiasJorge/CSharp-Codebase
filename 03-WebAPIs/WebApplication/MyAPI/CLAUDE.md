# CLAUDE.md — MyAPI (WebApplication)

API WeatherForecast com EF Core + PostgreSQL e autenticação JWT. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 03-WebAPIs/WebApplication/MyAPI/MyAPI.csproj
dotnet run --project 03-WebAPIs/WebApplication/MyAPI/MyAPI.csproj
```

## Estrutura interna

- `AppDbContext.cs` + `Migrations/` — migração `InitialCreate` versionada com snapshot. Mudanças no modelo pedem nova migração.
- `Controllers/AuthController.cs` — emissão de JWT.
- `Controllers/WeatherForecastController.cs` — CRUD protegido.
- `Middleware/RequestResponseLoggingMiddleware.cs` — logging do par requisição/resposta.

## Pontos de atenção

- **Exige PostgreSQL ativo** (connection string em `appsettings.json`):

  ```bash
  docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres
  ```
- TFM `net9.0`. `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.3, `JwtBearer` 9.0.1.
- A chave de assinatura JWT está em `appsettings.json` — é material de demonstração, não reutilize valores.
- O README local tem caracteres corrompidos (mojibake de acentuação). Se for reescrevê-lo, grave em UTF-8.
- O diretório-pai `WebApplication/` agrupa um único projeto; comandos sempre para o `.csproj` interno.
