# CLAUDE.md — ApiKeyAuthenticationDemo

Minimal API com esquema de autenticação próprio para chaves de API: handler, hash, comparação em tempo constante, escopos, rotação e revogação. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/ApiKeyAuthenticationDemo/ApiKeyAuthenticationDemo.csproj
dotnet run --project 04-Authentication/ApiKeyAuthenticationDemo/ApiKeyAuthenticationDemo.csproj
```

Sobe em `http://localhost:5273`. Roteiro em `ApiKeyAuthenticationDemo.http`; o README traz a sequência em curl e a matriz de status esperada.

## Estrutura interna

`Authentication/ApiKeyAuthenticationHandler` é o assunto do projeto. Dois pontos de decisão:

- **`NoResult()` quando o header não veio, `Fail()` quando veio credencial ruim.** Com mais de um esquema registrado, `Fail()` encerra a cadeia e impede outro esquema de tentar. Trocar um pelo outro não quebra este sample (há um esquema só) mas ensina o oposto do certo.
- `HandleChallengeAsync` escreve o `WWW-Authenticate`; sem ele o 401 sai mudo.

`Keys/ApiKeyStore` guarda o formato do token: `cbk_<id>_<segredo>`. O id público é o que permite `TryGetValue` em vez de varrer todas as chaves. Como existe comparação de segredo depois da busca, `CryptographicOperations.FixedTimeEquals` é necessário **aqui** — e o contraste com `RefreshTokenRotationDemo` (que busca pelo hash inteiro e não compara nada) está documentado nos dois READMEs. É um par deliberado; preservá-lo.

`Rotate` dá `RotationGracePeriod` (10 min) à chave antiga em vez de invalidá-la na hora.

## Pontos de atenção

- TFM `net10.0`. **Sem pacote externo** — `AuthenticationHandler` e `CryptographicOperations` são do framework. Template `web` (Minimal API).
- O `SHA-256` sem KDF segue a mesma justificativa de [RefreshTokenRotationDemo](../RefreshTokenRotationDemo/CLAUDE.md): segredo de 256 bits aleatórios não é alvo de força bruta. Está comentado no código.
- **Não substituir `FixedTimeEquals` por `==`.** É a linha que o projeto existe para ensinar, e a troca não quebra nenhum teste — só reintroduz o canal lateral de tempo.
- Os endpoints `/admin/*` estão **abertos de propósito**, para o exemplo rodar por curl. Está dito no código e no README. Não "consertar" adicionando autenticação: isso tornaria o roteiro do README impossível de executar sem um bootstrap manual.
- O prefixo `cbk` não é enfeite: habilita secret scanning. Mudar o formato do token invalida o diagrama de anatomia no README.
- Rotação com sobreposição de 10 minutos é curta para produção de propósito (lá seriam dias), para caber num teste manual. Alterar `RotationGracePeriod` invalida o texto do endpoint e do README, que citam 10 minutos em três lugares.
- 401 é credencial ausente/inválida; 403 é chave válida sem o escopo. A matriz do README depende dessa distinção.
- Estado em memória: reiniciar o processo apaga todas as chaves emitidas.
- **Fronteira com os vizinhos**: `RefreshTokenRotationDemo` cobre credencial de usuário com sessão; aqui é credencial de serviço, de vida longa. `PolicyBasedAuthorizationDemo` aprofunda autorização — aqui ela fica no mínimo (`RequireClaim`) de propósito, para não desviar o foco do handler de autenticação.
