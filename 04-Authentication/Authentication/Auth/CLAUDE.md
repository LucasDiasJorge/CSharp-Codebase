# CLAUDE.md — Auth

API mínima com emissão e validação de JWT. É a base de referência da trilha. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/Authentication/Auth/Auth.csproj
dotnet run --project 04-Authentication/Authentication/Auth/Auth.csproj
```

Requisições de exemplo em `Auth.http`.

## Estrutura interna

- `Services/AuthService.cs` — valida credenciais e monta o token (claims, expiração, assinatura).
- `Controllers/AuthController.cs` — endpoint de login.
- `Controllers/ExampleController.cs` — endpoint protegido, que só existe para provar que o token funciona.
- `Middlewares/Extensions/UseCustomMiddlewaresExtensions.cs` — agrupa registro de middleware.

É o exemplo mais enxuto do fluxo JWT; `AdvancedAuthSystem` (policies), `SessionManagement` (refresh token) e `OAuthApplication` (provedor externo) partem daqui.

## Pontos de atenção

- TFM `net9.0`. `JwtBearer` 9.0.4, `System.IdentityModel.Tokens.Jwt` 8.8.0.
- Credenciais e chave de assinatura são **fixas no código/configuração**, para manter o exemplo autocontido. Não reaproveite os valores.
