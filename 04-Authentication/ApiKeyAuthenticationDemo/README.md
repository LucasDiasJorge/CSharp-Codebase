# ApiKeyAuthenticationDemo

API que implementa um esquema de autenticação próprio para chaves de API, cobrindo formato do token, armazenamento por hash, comparação em tempo constante, escopos, rotação com sobreposição e revogação.

## Visão geral

Chave de API é a credencial de serviço para serviço: sem usuário na frente da tela, sem fluxo interativo, sem token de curta duração para renovar. Em compensação, ela vive por muito tempo em arquivos de configuração e variáveis de ambiente — e é dali que costuma vazar.

O exemplo escreve um `AuthenticationHandler` do zero. É o ponto em que uma credencial qualquer vira um `ClaimsPrincipal`; daí em diante, todo o resto do ASP.NET Core funciona sem saber o que havia no header. Os escopos da chave viram claims, e a autorização acontece por policy, exatamente como aconteceria com um JWT.

O formato do token carrega a decisão mais importante: `cbk_<id>_<segredo>`. O id viaja em claro e permite buscar a chave diretamente, em vez de comparar o segredo contra todas as cadastradas. O prefixo fixo identifica o tipo de credencial em um log e permite que ferramentas de secret scanning reconheçam a chave e avisem quando ela vazar para um repositório.

Por buscar pelo id e só então conferir o segredo, existe uma comparação de verdade a proteger — e é por isso que `CryptographicOperations.FixedTimeEquals` aparece aqui e não em `RefreshTokenRotationDemo`, que localiza o token pelo hash inteiro e nunca compara nada.

## Conceitos abordados

- `AuthenticationHandler<TOptions>` próprio e registro do esquema.
- `AuthenticateResult.NoResult()` versus `Fail()` versus `Success()`.
- `HandleChallengeAsync` e o header `WWW-Authenticate`.
- Formato de token com prefixo e identificador público.
- Armazenamento do hash do segredo, nunca do segredo.
- Comparação em tempo constante com `CryptographicOperations.FixedTimeEquals`.
- Escopos como claims e autorização por policy.
- Rotação com período de sobreposição e revogação imediata.
- `lastUsedAt` como instrumento para encontrar chaves esquecidas.

## Objetivos de aprendizagem

- Escrever um esquema de autenticação e integrá-lo ao pipeline sem gambiarra em middleware.
- Saber quando devolver `NoResult` em vez de `Fail`, e por que a diferença importa.
- Projetar o formato de uma chave pensando em busca, diagnóstico e detecção de vazamento.
- Identificar quando comparação em tempo constante é necessária de fato.
- Planejar rotação que não derrube clientes que ainda não atualizaram.

## Estrutura do projeto

```text
ApiKeyAuthenticationDemo/
|-- Authentication/
|   `-- ApiKeyAuthenticationHandler.cs
|-- Keys/
|   |-- ApiKey.cs
|   `-- ApiKeyStore.cs
|-- Properties/
|   `-- launchSettings.json
|-- ApiKeyAuthenticationDemo.csproj
|-- ApiKeyAuthenticationDemo.http
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 04-Authentication/ApiKeyAuthenticationDemo/ApiKeyAuthenticationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 04-Authentication/ApiKeyAuthenticationDemo/ApiKeyAuthenticationDemo.csproj
```

A API sobe em `http://localhost:5273` e não exige serviço externo.

Roteiro completo:

```bash
# emitir uma chave so de leitura
CHAVE=$(curl -s -X POST http://localhost:5273/admin/keys \
  -H "Content-Type: application/json" \
  -d '{"owner":"servico-relatorios","scopes":["data:read"]}' \
  | grep -o '"apiKey":"[^"]*"' | cut -d'"' -f4)

curl -s -o /dev/null -w "%{http_code}\n" \
  -H "X-Api-Key: $CHAVE" http://localhost:5273/data/read    # 200

curl -s -o /dev/null -w "%{http_code}\n" -X POST \
  -H "X-Api-Key: $CHAVE" http://localhost:5273/data/write   # 403 - falta o escopo
```

## Boas práticas e pontos de atenção

- Devolva `NoResult()` quando o header não veio, e `Fail()` só quando veio uma credencial inválida. Com mais de um esquema registrado, `Fail()` encerra a cadeia e impede que outro esquema tente autenticar a requisição.
- Guarde o hash, nunca o segredo. Não deve existir endpoint que reexiba a chave — se existir, é porque ela está guardada em claro em algum lugar.
- Use comparação em tempo constante quando houver comparação de segredo. `==` sobre string retorna no primeiro byte diferente, e essa diferença de tempo, medida em muitas tentativas, revela o segredo byte a byte. O custo de `FixedTimeEquals` é irrelevante.
- Ponha um prefixo fixo no token. Além de identificar o tipo de credencial em um log, é o que permite a ferramentas de secret scanning detectar a chave vazada em um repositório.
- Não use o segredo inteiro como chave de busca se quiser exibir a lista de chaves ao usuário. O id público separa "qual chave é" de "prove que é ela".
- Rotação precisa de sobreposição. Emitir a nova e invalidar a antiga no mesmo instante derruba todo cliente que ainda não trocou a configuração. Aqui o prazo é de 10 minutos; em produção seria de dias.
- Dê escopos mínimos. A chave de um serviço que só lê não deveria conseguir escrever, e esse controle não custa nada quando escopo vira claim e a autorização vira policy.
- Acompanhe `lastUsedAt`. Chave criada para um teste e nunca revogada é risco sem contrapartida — e só aparece se alguém olhar o último uso.
- Distinga 401 de 403. Chave ausente ou inválida é 401; chave válida sem o escopo necessário é 403. Confundir os dois esconde do cliente o que ele precisa corrigir.
- Os endpoints `/admin/*` estão abertos aqui para o exemplo ser executável por curl. Em produção, a administração de chaves precisa de autenticação própria e mais forte que a das chaves que ela emite.

## Conteúdo complementar

Endpoints:

| Rota | Finalidade |
|---|---|
| `POST /admin/keys` | Emite chave; devolve o valor em claro uma única vez |
| `POST /admin/keys/{id}/rotate` | Emite a sucessora e dá 10 minutos à antiga |
| `DELETE /admin/keys/{id}` | Revoga imediatamente |
| `GET /admin/keys` | Lista metadados, nunca segredos |
| `GET /data/public` | Aberto |
| `GET /data/read` | Exige escopo `data:read` |
| `POST /data/write` | Exige escopo `data:write` |

Matriz observada com uma chave de escopo `data:read`:

| Requisição | Status | Motivo |
|---|---|---|
| `GET /data/public` sem chave | 200 | Endpoint aberto |
| `GET /data/read` sem chave | 401 | Nenhuma credencial apresentada |
| `GET /data/read` com a chave | 200 | Autenticada e com o escopo |
| `POST /data/write` com a chave | 403 | Autenticada, mas sem o escopo |
| `GET /data/read` com chave adulterada | 401 | Segredo não confere |
| `GET /data/read` com formato inválido | 401 | Não é uma chave |

Resposta do 401:

```text
HTTP/1.1 401 Unauthorized
WWW-Authenticate: ApiKey realm="ApiKeyAuthenticationDemo", header="X-Api-Key"
```

Rotação, passo a passo observado:

```text
rotate(chaveA)        -> emite chaveB; chaveA ganha expiresAt = agora + 10min
GET /data/read (A)    -> 200   (ainda no prazo de graca)
GET /data/read (B)    -> 200
DELETE chaveA         -> revogada
GET /data/read (A)    -> 401   (revogacao tem efeito imediato)
GET /data/read (B)    -> 200
```

Anatomia do token:

```text
cbk_dl3hCGgmizs_lHtLbRNSGjlR8qU2iJAJl8pfRZJP-pXfJvvP0oYjKUQ
^^^ ^^^^^^^^^^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
 |       |                        |
 |       |                        +-- segredo, 256 bits; so o hash e guardado
 |       +-- id publico; e por ele que a chave e localizada
 +-- prefixo fixo; identifica o tipo e habilita secret scanning
```

Quando usar comparação em tempo constante — comparando com o projeto vizinho:

| Projeto | Como localiza a credencial | Compara segredo? | Precisa de tempo constante? |
|---|---|---|---|
| `RefreshTokenRotationDemo` | Busca pelo hash do token inteiro | Não | Não |
| `ApiKeyAuthenticationDemo` | Busca pelo id público | Sim, o hash do segredo | Sim |

A diferença não é de rigor, e sim de estrutura: onde não há comparação, não há canal lateral de tempo a proteger.

Relação com os vizinhos da trilha: `RefreshTokenRotationDemo` trata de credencial de usuário, com sessão e renovação; aqui a credencial é de serviço, de vida longa e sem usuário presente. `PolicyBasedAuthorizationDemo` aprofunda a autorização que este projeto usa de forma simples, via `RequireClaim`.

## Referências e documentação complementar

- https://learn.microsoft.com/aspnet/core/security/authentication/
- https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.authentication.authenticationhandler-1
- https://learn.microsoft.com/dotnet/api/system.security.cryptography.cryptographicoperations.fixedtimeequals
- https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html
