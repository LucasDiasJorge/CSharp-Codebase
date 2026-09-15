# CLAUDE.md — PolicyBasedAuthorizationDemo

API sobre autorização por policies: claims, requirements, handlers e autorização por recurso. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/PolicyBasedAuthorizationDemo/PolicyBasedAuthorizationDemo.csproj
dotnet run --project 04-Authentication/PolicyBasedAuthorizationDemo/PolicyBasedAuthorizationDemo.csproj
```

Sobe em `http://localhost:5079`. `POST /dev/token` com `{"user":"ana|bruno|carla"}` devolve o token; requisições prontas em `PolicyBasedAuthorizationDemo.http`. A matriz esperada de status está no README.

## Estrutura interna

`Authorization/Requirements/` guarda só dados; `Authorization/Handlers/` guarda as decisões. A divisão é o assunto do projeto, não organização de pastas.

**`DocumentEditRequirement` tem dois handlers** (`DocumentOwnerHandler` e `DocumentAdminHandler`) registrados em `Program.cs`. Isso é OU: basta um chamar `Succeed`. `SeniorEngineering`, em contraste, tem dois **requirements** na mesma policy — isso é E. Os dois casos existem lado a lado de propósito; não unificar.

`MinimumClearanceHandler` **deliberadamente não chama `context.Fail()`** quando o nível é insuficiente — apenas retorna. Isso mantém a exigência pendente e permite que outro handler a satisfaça. Trocar por `Fail()` seria uma mudança de semântica silenciosa: `Fail()` vence qualquer `Succeed`.

`DocumentsController.Edit` chama `IAuthorizationService.AuthorizeAsync` no meio da ação, depois de carregar o documento — é o caso que atributo não resolve.

## Pontos de atenção

- TFM `net10.0` (a trilha é majoritariamente `net9.0`): a máquina não tem o runtime ASP.NET Core 9.0 instalado e este sample precisa ser executado. Mesma decisão de [RefreshTokenRotationDemo](../RefreshTokenRotationDemo/CLAUDE.md).
- Pacote: `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12, além do `Microsoft.AspNetCore.OpenApi` do template.
- **`options.MapInboundClaims = false` e `NameClaimType = "unique_name"` são obrigatórios aqui.** Sem eles, o handler JWT remapeia `sub` para o schema longo do WS-Federation, `User.FindFirst("sub")` volta nulo e `DocumentOwnerHandler` passa a negar tudo — sem erro, sem log, só 403 onde deveria haver 200. Se a autorização por dono parar de funcionar, comece por aqui.
- A policy `WeekdayOnly` **depende do dia em que o teste roda**: 200 de segunda a sexta, 403 no fim de semana. A tabela do README está redigida nesses termos; não transformar em valor fixo nem em teste automatizado.
- Autenticação é andaime: `Users/DemoTokenIssuer` emite tokens de uma hora para três perfis fixos, sem senha. Não expandir — ciclo de vida de token é assunto de `RefreshTokenRotationDemo`, e handler de autenticação é de `ApiKeyAuthenticationDemo`.
- A chave de assinatura é constante no código, com nome que deixa claro ser de desenvolvimento. É aceitável neste sample porque não há segredo real; não copiar o padrão para projeto que vá a algum lugar.
- `Edit` responde `Forbid()` (403), não 404. A escolha está comentada no código: esconder a existência do recurso é defensável, mas precisa ser deliberado.
- **Fronteira com os vizinhos**: `Authentication/Auth` e `AdvancedAuthSystem` cobrem autenticação. Este projeto trata só de autorização e começa com o usuário já identificado.
