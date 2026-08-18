# CLAUDE.md — SessionManagement

Gestão de sessão com **refresh token rotation** e controle multi-dispositivo. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/SessionManagement/SessionManagement.csproj
dotnet run --project 04-Authentication/SessionManagement/SessionManagement.csproj
```

Requisições de exemplo em `SessionManagement.http`.

## Estrutura interna

- `Services/SessionService.cs` — o núcleo: cria, lista e revoga sessões por dispositivo. A **rotação** significa que cada uso do refresh token o invalida e emite um novo; reuso de token antigo indica roubo e derruba a cadeia.
- `Services/TokenService.cs` — emissão dos tokens de acesso e refresh.
- `Middleware/SessionValidationMiddleware.cs` — valida a sessão em cada requisição, não só a assinatura do JWT. É o que permite revogação imediata, coisa que JWT puro não oferece.
- Persistência dupla: **Redis** (`StackExchange.Redis`) para sessões e EF Core InMemory para usuários.

## Pontos de atenção

- **BUILD QUEBRADO.** O `.csproj` não declara `TargetFramework` (dependia do `Directory.Build.props` removido no commit `50763d5`); `dotnet build` falha com `NETSDK1013`. Declare `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>` e `<ImplicitUsings>enable</ImplicitUsings>`, ou restaure o props na raiz.
- **Exige Redis ativo**:

  ```bash
  docker run -d --name redis -p 6379:6379 redis
  ```
- Avança sobre `Auth` (JWT simples): a diferença é justamente sessão revogável versus token stateless.
