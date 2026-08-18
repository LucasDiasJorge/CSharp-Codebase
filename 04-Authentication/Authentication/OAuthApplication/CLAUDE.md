# CLAUDE.md — OAuthApplication

API que autentica contra um **provedor externo** via OAuth 2.0 / OpenID Connect. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/Authentication/OAuthApplication/OAuthApplication.csproj
dotnet run --project 04-Authentication/Authentication/OAuthApplication/OAuthApplication.csproj
```

## Estrutura interna

- `Services/AuthService.cs` (+ `IAuthService`) — conduz o fluxo de autorização.
- `Services/TokenValidationService.cs` (+ `ITokenValidationService`) — valida o token **emitido por terceiro**. Essa separação é o ponto do exemplo: com OAuth você valida token que não emitiu, o que exige buscar as chaves públicas do provedor, e não uma chave simétrica local (como faz `Auth`).
- `AuthController.cs` — endpoints de callback/login.

## Pontos de atenção

- **Exige um provedor OAuth configurado** (client id, secret e authority em `appsettings.json`). Sem credenciais válidas o fluxo não completa — o projeto compila, mas não autentica de ponta a ponta.
- TFM `net9.0`. `Authentication.OpenIdConnect` 9.0.2, `JwtBearer` 9.0.2, `Newtonsoft.Json` 13.0.3.
- Nunca comite client secret real neste arquivo.
