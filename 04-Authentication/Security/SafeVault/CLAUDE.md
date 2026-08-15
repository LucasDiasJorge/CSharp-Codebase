# CLAUDE.md — SafeVault

Web API de cofre de segredos, orientada a defesa contra vulnerabilidades comuns. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/Security/SafeVault/SafeVault.csproj
dotnet run --project 04-Authentication/Security/SafeVault/SafeVault.csproj
```

## Estrutura interna

A defesa é feita em camadas, e o exemplo existe para mostrar essa sobreposição:

- `Middleware/` — `SecurityHeadersMiddleware`, `RateLimitingMiddleware`, `ExceptionHandlingMiddleware` (evita vazar stack trace), `RequestLoggingMiddleware`.
- `Security/InputValidator.cs` — validação/sanitização contra injeção e XSS.
- `Security/PasswordHasher.cs` — BCrypt.
- `Data/` — `DbConnectionFactory`, `UserRepository`, `SecretRepository` com **Dapper e queries parametrizadas** (a defesa concreta contra SQL injection).
- `Services/AuthService.cs`, `SecretService.cs` — regras de negócio.

## Pontos de atenção

- **Exige SQL Server ativo** e o schema criado a partir de `setup-database.sql`. Connection string em `appsettings.json`.
- **TFM `net7.0`** — fora de suporte e a versão mais antiga da trilha. Os pacotes acompanham (`JwtBearer` 7.0.9, `Microsoft.Data.SqlClient` 5.1.1). Preserve o TFM ao editar; migrar exige atualizar todo o conjunto.
- `Tests/SecurityTests.cs` está **dentro do projeto web**, não em projeto de teste separado: `dotnet test` não roda esses testes.
- README local em inglês, diferente do padrão PT-BR do repositório. Mantenha o idioma do arquivo ao editá-lo.
