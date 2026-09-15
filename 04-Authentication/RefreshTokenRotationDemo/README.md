# RefreshTokenRotationDemo

API que emite access tokens curtos e refresh tokens rotativos de uso único, com detecção de reuso, revogação por família e armazenamento apenas do hash dos tokens.

## Visão geral

Um access token JWT é auto-contido: o servidor o valida pela assinatura, sem consultar nada. Isso é rápido, mas tem um preço — não há como revogá-lo antes de expirar. A resposta é mantê-lo curto e apoiar a sessão longa em um refresh token, que é opaco, fica registrado no servidor e pode ser invalidado a qualquer momento.

Aqui o refresh token vale exatamente uma troca. Cada `POST /auth/refresh` devolve um par novo e marca o token apresentado como usado. É isso que torna o vazamento detectável: se um token já trocado reaparece, existem duas cópias em circulação. Como não há como saber qual delas é do dono e qual é do atacante, a única saída segura é derrubar a linhagem inteira e exigir novo login.

Essa linhagem é a família. Login abre uma família; cada rotação acrescenta um elo. A detecção de reuso revoga todos os elos de uma vez, incluindo o token mais recente, que até então era perfeitamente válido — e é justamente esse efeito colateral que expulsa o atacante.

O servidor nunca guarda o refresh token em claro, só o SHA-256 dele. Quem obtiver uma cópia do banco não consegue usar token nenhum.

## Conceitos abordados

- Access token curto e auto-contido versus refresh token longo e revogável.
- Rotação: refresh token de uso único, invalidado no momento da troca.
- Detecção de reuso como sinal de vazamento, e revogação da família como resposta.
- Família de tokens (linhagem) ligando login e rotações sucessivas.
- Armazenamento do hash, nunca do token em claro.
- Por que SHA-256 basta para um token aleatório de 256 bits, e por que senha exige outra coisa.
- Janela deslizante por token e teto absoluto por família.
- `ClockSkew` do `JwtBearer` e seu efeito sobre a expiração real.
- `jti` como identificador único por token, para auditoria e denylist.

## Objetivos de aprendizagem

- Entender por que um JWT curto não substitui revogação, apenas limita o dano.
- Implementar rotação sem deixar janela em que dois refresh tokens valem ao mesmo tempo.
- Reconhecer que detecção de reuso exige guardar o token usado, não apagá-lo.
- Justificar a escolha do algoritmo de hash pelo tipo de segredo que se está protegendo.
- Evitar que rotação indefinida se transforme em sessão eterna.

## Estrutura do projeto

```text
RefreshTokenRotationDemo/
|-- Controllers/
|   `-- AuthController.cs
|-- Models/
|   `-- AuthModels.cs
|-- Tokens/
|   |-- RefreshOutcome.cs
|   |-- RefreshToken.cs
|   |-- RefreshTokenStatus.cs
|   |-- RefreshTokenStore.cs
|   `-- TokenService.cs
|-- Users/
|   `-- UserStore.cs
|-- Properties/
|   `-- launchSettings.json
|-- Program.cs
|-- README.md
|-- RefreshTokenRotationDemo.csproj
`-- RefreshTokenRotationDemo.http
```

## Como executar

```bash
dotnet run --project 04-Authentication/RefreshTokenRotationDemo/RefreshTokenRotationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 04-Authentication/RefreshTokenRotationDemo/RefreshTokenRotationDemo.csproj
```

A API sobe em `http://localhost:5286` e não exige serviço externo. Usuários do exemplo: `ana` / `senha123` (admin) e `bruno` / `senha123` (reader).

Roteiro que demonstra a detecção de reuso:

```bash
# 1. login
curl -s -X POST http://localhost:5286/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"ana","password":"senha123"}'

# 2. troque o refreshToken recebido (guarde o valor antigo)
curl -s -X POST http://localhost:5286/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<TOKEN_DO_PASSO_1>"}'

# 3. reenvie o token JA USADO do passo 1
curl -s -X POST http://localhost:5286/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<O_MESMO_TOKEN_DO_PASSO_1>"}'
```

O passo 3 responde 401 `ReuseDetected`, e a partir daí nem o token mais recente funciona. `GET /auth/tokens` mostra a cadeia inteira com o estado de cada elo.

## Boas práticas e pontos de atenção

- Marque o token como usado antes de emitir o próximo. Se a ordem se inverter e a emissão falhar, o token antigo continua ativo e a rotação deixa de ser de uso único.
- Não apague o refresh token trocado. A detecção de reuso depende de encontrá-lo com estado `Used`; apagando, um replay vira apenas "token desconhecido" e o vazamento passa despercebido.
- Revogue a família, não o token. Revogar só o token apresentado deixa o atacante com o elo mais recente, que é exatamente o que ele quer.
- Guarde apenas o hash. E note que aqui SHA-256 puro é a escolha certa: BCrypt, Argon2 e afins existem para segredos de baixa entropia, onde força bruta é viável. Um valor aleatório de 256 bits não é adivinhável, então um KDF em toda requisição custaria caro sem comprar nada. Para senha, a conclusão é a oposta.
- A busca é por chave de hash, não comparação byte a byte do segredo — por isso não há canal lateral de tempo a proteger neste ponto.
- Dê um teto absoluto à família. Sem ele, cada rotação empurra a validade para frente e a sessão nunca termina.
- `ClockSkew` padrão do `JwtBearer` é de cinco minutos. Com access token de 60 segundos, isso tornaria a expiração invisível; em produção, estende silenciosamente a vida de todo token emitido.
- Responda ao reuso com o mesmo 401 das outras falhas. O cliente legítimo não precisa saber que a detecção existe, e o atacante muito menos.
- O endpoint `GET /auth/tokens` não teria lugar em produção: ele existe para tornar o mecanismo observável.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /auth/login` | Autentica e abre uma família de tokens |
| `POST /auth/refresh` | Rotaciona: invalida o token apresentado e emite um par novo |
| `POST /auth/logout` | Revoga a família inteira |
| `GET /auth/me` | Endpoint protegido, exige access token válido |
| `GET /auth/tokens` | Estado da cadeia de tokens (apenas para o exemplo) |

Ciclo de vida de um refresh token:

| Estado | Como se chega | O que acontece ao apresentá-lo |
|---|---|---|
| `Active` | Emitido no login ou em uma rotação | Troca aceita; passa a `Used` |
| `Used` | Já foi trocado uma vez | 401 `ReuseDetected` e revogação da família |
| `Revoked` | Logout ou detecção de reuso na família | 401 `Revoked` |

Sequência observada no roteiro acima:

```text
login            -> R1 Active
refresh(R1)      -> R1 Used, R2 Active
refresh(R2)      -> R2 Used, R3 Active
refresh(R1)      -> 401 ReuseDetected; R1, R2 e R3 passam a Revoked
refresh(R3)      -> 401 Revoked   (era valido ate o passo anterior)
```

Resposta real do replay:

```json
{
  "error": "ReuseDetected",
  "detail": "Refresh token ja utilizado. A familia 5cc9187c... foi revogada (2 tokens)."
}
```

Tempos configurados:

| Parâmetro | Valor no exemplo | Valor típico em produção |
|---|---|---|
| Access token | 60 segundos | 5 a 15 minutos |
| Refresh token | 7 dias | 7 a 30 dias |
| Família (teto absoluto) | 30 dias | 30 a 90 dias |
| `ClockSkew` | zero | zero a 30 segundos |

Os 60 segundos do access token são curtos de propósito, para que a expiração seja observável sem esperar.

Relação com os vizinhos da trilha: `Authentication/Auth` cobre a emissão e validação básica de JWT; `AdvancedAuthSystem` monta um sistema de autenticação mais completo. Este projeto trata de um único problema que os dois deixam em aberto: o que fazer quando o token de longa duração vaza.

## Referências e documentação complementar

- https://datatracker.ietf.org/doc/html/rfc6749#section-1.5
- https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics
- https://learn.microsoft.com/aspnet/core/security/authentication/jwt
- https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html
