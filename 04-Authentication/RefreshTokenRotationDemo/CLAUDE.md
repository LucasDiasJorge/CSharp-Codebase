# CLAUDE.md — RefreshTokenRotationDemo

API com access token curto e refresh token rotativo de uso único, detecção de reuso e revogação por família. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/RefreshTokenRotationDemo/RefreshTokenRotationDemo.csproj
dotnet run --project 04-Authentication/RefreshTokenRotationDemo/RefreshTokenRotationDemo.csproj
```

Sobe em `http://localhost:5286`. Usuários: `ana`/`senha123` (admin), `bruno`/`senha123` (reader). Roteiro completo em `RefreshTokenRotationDemo.http`; o README traz a sequência em curl.

## Estrutura interna

`Tokens/TokenService` concentra as três políticas: access token de 60s, rotação de uso único e detecção de reuso. `Rotate` tem uma ordem que não pode mudar — `stored.MarkUsed(now)` acontece **antes** de emitir o próximo par; invertendo, uma falha na emissão deixaria o token antigo ativo e a rotação deixaria de ser de uso único.

`Tokens/RefreshTokenStore` indexa por hash e nunca vê o token em claro depois de gerado. `RevokeFamily` é a resposta à detecção: varre a linhagem pelo `FamilyId` e derruba todos os elos, inclusive o mais recente.

`RefreshToken` guarda `ParentTokenHash` para reconstruir a cadeia, e dois prazos distintos: `ExpiresAt` (janela deslizante do token) e `FamilyExpiresAt` (teto absoluto da sessão).

`Controllers/AuthController.Tokens()` expõe o estado da cadeia — existe só para o exemplo.

## Pontos de atenção

- TFM `net10.0` (a trilha é majoritariamente `net9.0`): a máquina não tem o runtime ASP.NET Core 9.0 instalado e este sample precisa ser executado. Mesma decisão de [ApiVersioningDemo](../../03-WebAPIs/ApiVersioningDemo/CLAUDE.md).
- Pacote: `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12, além do `Microsoft.AspNetCore.OpenApi` do template.
- **Não apague o refresh token trocado.** A detecção de reuso depende de encontrá-lo com estado `Used`. Se alguém "limpar" tokens usados, o replay vira apenas `Unknown` e o vazamento passa despercebido — a funcionalidade central do projeto desaparece sem quebrar nenhum build.
- **`ClockSkew = TimeSpan.Zero` é deliberado.** O padrão do `JwtBearer` é 5 minutos de tolerância, o que tornaria os 60s do access token indistinguíveis de 6 minutos. Não remover.
- O `SHA-256` puro em `HashToken` **não é descuido**. Está comentado no código e explicado no README: KDF (BCrypt/Argon2) serve para segredo de baixa entropia; para token aleatório de 256 bits o custo não compra nada. Se alguém "corrigir" isso para BCrypt, o refresh fica caro à toa — e o comentário que explica a decisão se perde.
- Revogação é **por família**, nunca só pelo token apresentado. Revogar apenas o token deixaria o atacante com o elo mais recente.
- Estado em memória (`ConcurrentDictionary` em singleton): reiniciar o processo apaga todas as famílias.
- O access token de 60s torna a sessão do exemplo curta de propósito. Ao testar `GET /auth/me` manualmente, refaça o login se demorar — não é bug.
- `GET /auth/tokens` expõe prefixos de hash e a topologia da sessão; é endpoint de demonstração e não teria lugar em produção.
- **Fronteira com os vizinhos**: `Authentication/Auth` cobre emissão e validação básica de JWT e `AdvancedAuthSystem` monta um sistema completo com BCrypt e EF. Este projeto trata só do ciclo de vida do refresh token — não expandir para cadastro de usuário, hash de senha ou persistência.
