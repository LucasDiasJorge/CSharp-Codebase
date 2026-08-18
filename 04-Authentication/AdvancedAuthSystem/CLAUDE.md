# CLAUDE.md — AdvancedAuthSystem

API com autenticação JWT e **autorização baseada em policies e requirements**. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/AdvancedAuthSystem/AdvancedAuthSystem.csproj
dotnet run --project 04-Authentication/AdvancedAuthSystem/AdvancedAuthSystem.csproj
```

Requisições de exemplo em `AdvancedAuthSystem.http`.

## Estrutura interna

**Todos os arquivos são achatados na raiz com prefixo `AdvancedAuthSystem.`** — não há pastas `Controllers/`, `Services/`, `Models/`. O ponto de entrada é `AdvancedAuthSystem.Program.cs`.

O núcleo didático é a autorização, não a autenticação:

- `AdvancedAuthSystem.Requirements.cs` — os requisitos (o *que* precisa ser satisfeito).
- `AdvancedAuthSystem.Handlers.cs` — os handlers que avaliam cada requisito (o *como*).
- `AdvancedAuthSystem.PolicyNames.cs` — nomes de policy centralizados, evitando string mágica espalhada.
- `AdvancedAuthSystem.TokenService.cs` / `AuthService.cs` / `PasswordHasher.cs` — emissão de token e hash com BCrypt.
- `AdvancedAuthSystem.ResourceController.cs` — endpoints protegidos por cada policy.

Esse trio requirement + handler + policy name é o padrão que o exemplo ensina; mantenha-o ao estender.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` (dependia do `Directory.Build.props` removido no commit `50763d5`); `dotnet build` falha com `NETSDK1013`. Corrija declarando `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>`, ou restaure o props na raiz — lista dos 10 projetos afetados no [CLAUDE.md](../../CLAUDE.md) raiz.
- Há **dois** arquivos de launch settings (`Properties/launchSettings.json` e `AdvancedAuthSystem.launchSettings.json`); só o de `Properties/` é lido pelo SDK.
- Persistência via EF Core **InMemory**: usuários somem a cada restart.
