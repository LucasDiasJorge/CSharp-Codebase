# CLAUDE.md — MongoUserApi

API com MongoDB, CRUD de usuários, JWT e autorização por policies. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
docker run -d --name mongo -p 27017:27017 mongo

dotnet build 09-Data/Data/MongoUserApi/MongoUserApi.csproj
dotnet run --project 09-Data/Data/MongoUserApi/MongoUserApi.csproj
```

## Estrutura interna

- `Configuration/MongoSettings.cs` + `MongoContext.cs` — configuração tipada e acesso às coleções. Não há `DbContext` do EF Core: o driver do Mongo é usado direto.
- `Repositories/UserRepository.cs` (+ interface) — operações sobre a coleção.
- `Services/UserService.cs`, `TokenService.cs` (+ interfaces) — regras e emissão de JWT.
- `GlobalUsings.cs` — usings centralizados; se um tipo parecer não declarado, confira aqui antes de adicionar `using`.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` (dependia do `Directory.Build.props` removido no commit `50763d5`); falha com `NETSDK1013`. O README diz ".NET 8" e os pacotes são 8.0.x → `net8.0` é o alvo coerente ao corrigir. Lista dos 10 afetados no [CLAUDE.md](../../../CLAUDE.md) raiz.
- **Exige MongoDB ativo**; connection string em `appsettings.json`.
- `MongoDB.Driver` 2.27.0 — a linha 3.x mudou a API; siga o código existente.
