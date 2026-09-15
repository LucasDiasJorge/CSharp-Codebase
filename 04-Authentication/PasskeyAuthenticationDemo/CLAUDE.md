# CLAUDE.md — PasskeyAuthenticationDemo

Minimal API com as duas cerimônias do WebAuthn/FIDO2: registro de passkey e login por assinatura. Regras globais em [CLAUDE.md](../../CLAUDE.md).

## Comandos

```bash
dotnet build 04-Authentication/PasskeyAuthenticationDemo/PasskeyAuthenticationDemo.csproj
dotnet run --project 04-Authentication/PasskeyAuthenticationDemo/PasskeyAuthenticationDemo.csproj
```

Sobe em `http://localhost:5078`. A raiz serve a página com `navigator.credentials`.

## Como testar de verdade

**As cerimônias não rodam por curl** — WebAuthn exige um autenticador. Sem biometria na máquina, use o autenticador virtual do Chrome: DevTools → More tools → **WebAuthn** → *Enable virtual authenticator environment* → **Add** com *Supports resident keys* ligado. Depois, na página: Registrar passkey → Entrar sem usuário.

Por curl dá para verificar só a metade do servidor que não depende de assinatura:

```bash
curl -s -X POST http://localhost:5078/passkey/register/begin -H "Content-Type: application/json" -d '{"username":"ana"}'
curl -s http://localhost:5078/passkey/credentials
```

## Estrutura interna

`Endpoints/PasskeyEndpoints` tem os quatro passos (begin/complete de cada cerimônia). O ponto crítico está em `complete`: `OriginalOptions` vem de `store.TakeCeremony(...)`, ou seja, do que o **servidor** emitiu — nunca do que o cliente devolveu. Validar o desafio contra o valor do cliente tornaria a cerimônia decorativa sem quebrar nada visível.

`CredentialStore.TakeCeremony` **remove ao ler**: o desafio vale uma vez. Verificado — a segunda tentativa com o mesmo `ceremonyId` responde "cerimonia ja consumida".

`StoredCredential` guarda chave pública, `userHandle`, contador e `aaGuid`. Nada ali é secreto, e esse é o argumento do projeto.

`wwwroot/index.html` faz a conversão base64url ↔ `ArrayBuffer` nos dois sentidos — é onde integrações WebAuthn mais quebram.

## Pontos de atenção

- TFM `net10.0`. Pacote: `Fido2.AspNet` 4.0.1 (traz `Fido2` e `Fido2.Models`).
- **`options.Origins` está fixo em `http://localhost:5078` e precisa bater com a porta do `launchSettings.json`.** Se a porta mudar, a verificação falha com erro genérico de origem. O reflexo de "relaxar a validação" para destravar remove exatamente a proteção contra phishing — corrija a origem, não a validação.
- `ServerDomain` (RP ID) é `localhost`. Uma credencial criada em um domínio não funciona em outro: é o mecanismo, não um obstáculo.
- **Limite de verificação conhecido:** a verificação criptográfica de attestation/assertion nunca foi executada nesta máquina — exige autenticador real ou virtual em navegador. O que está confirmado por curl: geração das opções (desafio, `rp`, `user`, `pubKeyCredParams`), uso único da cerimônia, 404 para usuário sem passkey e `allowCredentials` vazio no fluxo usernameless. Ao mexer nos handlers de `complete`, teste no navegador — o compilador não cobre esse caminho.
- Os fluxos de `complete` dependem do formato exato que a página monta (`clientDataJson`, `attestationObject`, `signature` em base64url). Mexer no JS e no C# em separado quebra a integração silenciosamente.
- `GetOrCreateUserHandle` precisa continuar estável por usuário. Gerar handle novo a cada registro faria o autenticador tratar a mesma pessoa como outra conta.
- Estado em memória: reiniciar o processo apaga as credenciais registradas, e as passkeys que ficaram no autenticador passam a apontar para nada. No autenticador virtual do Chrome, remova e recrie o autenticador ao reiniciar o servidor.
- `AttestationPreference.None` é escolha deliberada e está comentada no código. Ligar atestação exige validar cadeias de certificado de fabricante — fora do escopo didático daqui.
- **Fronteira com os vizinhos**: `Authentication/Auth` e `AdvancedAuthSystem` autenticam por senha. Este projeto substitui a etapa de provar identidade e para por aí — sessão é assunto de [RefreshTokenRotationDemo](../RefreshTokenRotationDemo/CLAUDE.md), permissão é de [PolicyBasedAuthorizationDemo](../PolicyBasedAuthorizationDemo/CLAUDE.md).
