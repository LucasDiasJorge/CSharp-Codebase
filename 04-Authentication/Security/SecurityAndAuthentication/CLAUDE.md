# CLAUDE.md — SecurityAndAuthentication

Sistema completo de autenticação com JWT, RBAC e front-end estático incluso. Regras globais em [CLAUDE.md](../../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/Security/SecurityAndAuthentication/SecurityAndAuthentication.csproj
dotnet run --project 04-Authentication/Security/SecurityAndAuthentication/SecurityAndAuthentication.csproj
```

## Estrutura interna

Único projeto da trilha com **UI própria**: `Front/` traz `index.html`, `login.html`, `dashboard.html`, `admin.html` e `change-password.html`, servidos como arquivos estáticos e consumindo a própria API. Dá para exercitar o fluxo pelo navegador, sem cliente HTTP.

No backend:

- `Services/AuthService.cs` — login, hash BCrypt e mitigação de força bruta.
- `Authorization/SameUserOrAdminHandler.cs` — authorization handler que resolve "o próprio usuário **ou** um admin", regra que policy declarativa sozinha não expressa.
- `Data/ApplicationDbContext.cs` — EF Core InMemory.

## Pontos de atenção

- **Existem dois `User`**: `Models/User.cs` e `Data/Models/User.cs`. Confirme qual está em uso antes de editar — mexer no errado produz mudança sem efeito.
- O `.csproj` referencia um pacote **`EFCore` 1.1.2**, que não é o pacote oficial (`Microsoft.EntityFrameworkCore`) e destoa das demais dependências 9.0.7. Provável referência acidental; não replique.
- TFM `net9.0`. Dados em memória: usuários somem a cada restart.
- README local em inglês.
