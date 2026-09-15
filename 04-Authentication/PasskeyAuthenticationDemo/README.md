# PasskeyAuthenticationDemo

API que implementa autenticação com passkeys (WebAuthn/FIDO2): registro de credencial, login por assinatura e as duas propriedades que tornam o mecanismo resistente a phishing.

## Visão geral

Uma passkey é um par de chaves criado dentro do autenticador — Windows Hello, Touch ID, uma YubiKey. A chave privada nunca sai de lá; o servidor recebe e guarda apenas a chave pública. A consequência é direta: não existe segredo compartilhado para vazar. Um dump deste banco não permite entrar em conta nenhuma, porque não há nada ali que sirva para provar identidade.

São duas cerimônias, cada uma em dois passos. No registro, o servidor emite um desafio e o autenticador devolve a chave pública assinada. No login, o servidor emite outro desafio e o autenticador o assina com a chave privada; o servidor confere a assinatura com a chave que guardou.

A resistência a phishing vem do RP ID. A credencial fica presa ao domínio que a criou, e o autenticador se recusa a assiná-la para qualquer outro. Um site clone em `Iocalhost-banco.com` não obtém assinatura válida nem com o usuário totalmente convencido — não há nada que a vítima possa digitar ou colar que resolva o problema para o atacante. É a diferença estrutural para senha, OTP e código de SMS, que o usuário pode entregar a quem pedir.

O `userVerification` acrescenta o segundo fator sem cerimônia extra: a biometria ou o PIN destravam a chave localmente, então a autenticação já é posse do dispositivo mais algo que o usuário é ou sabe.

## Conceitos abordados

- Cerimônia de registro (attestation) e de autenticação (assertion), cada uma em dois passos.
- Desafio gerado pelo servidor, de uso único, conferido contra o que foi emitido.
- RP ID e verificação de origem como base da resistência a phishing.
- Par de chaves gerado no autenticador; servidor guarda apenas a chave pública.
- `userHandle` opaco em vez de e-mail dentro do autenticador.
- Contador de assinaturas e detecção de credencial clonada.
- Credenciais descobríveis (resident keys) e login sem digitar usuário.
- `excludeCredentials` para evitar registro duplicado no mesmo autenticador.
- `attestation: none` e quando a atestação do fabricante realmente importa.

## Objetivos de aprendizagem

- Descrever as duas cerimônias do WebAuthn e o que cada passo verifica.
- Explicar por que passkey resiste a phishing e OTP não.
- Entender o que o servidor precisa guardar — e o que ele deixa de precisar guardar.
- Reconhecer por que o desafio tem de ser validado contra o valor emitido pelo servidor.
- Avaliar o que o contador de assinaturas detecta e o que ele não garante.

## Estrutura do projeto

```text
PasskeyAuthenticationDemo/
|-- Credentials/
|   |-- CredentialStore.cs
|   `-- StoredCredential.cs
|-- Endpoints/
|   `-- PasskeyEndpoints.cs
|-- Properties/
|   `-- launchSettings.json
|-- wwwroot/
|   `-- index.html
|-- PasskeyAuthenticationDemo.csproj
|-- Program.cs
`-- README.md
```

## Como executar

```bash
dotnet run --project 04-Authentication/PasskeyAuthenticationDemo/PasskeyAuthenticationDemo.csproj
```

Para validar apenas a compilação:

```bash
dotnet build 04-Authentication/PasskeyAuthenticationDemo/PasskeyAuthenticationDemo.csproj
```

A aplicação sobe em `http://localhost:5078` e não exige serviço externo. Abra essa URL no navegador — **WebAuthn só funciona dentro do navegador**, e as cerimônias não podem ser executadas por curl.

Sem biometria disponível na máquina, use o autenticador virtual do Chrome:

1. Abra o DevTools (F12).
2. Menu do DevTools → **More tools → WebAuthn**.
3. Marque **Enable virtual authenticator environment**.
4. **Add** um autenticador com **Supports resident keys** e **Supports user verification** ligados.
5. Volte à página e clique em **Registrar passkey**, depois em **Entrar sem usuário**.

O que dá para inspecionar sem autenticador:

```bash
curl -s -X POST http://localhost:5078/passkey/register/begin \
  -H "Content-Type: application/json" -d '{"username":"ana"}'

curl -s http://localhost:5078/passkey/credentials
```

O primeiro mostra o desafio e as opções emitidas; o segundo mostra o que o servidor guarda depois de um registro — e o ponto é que não há nada secreto ali.

## Boas práticas e pontos de atenção

- Valide o desafio contra o que o **servidor** emitiu, recuperado do armazenamento. Aceitar o desafio que o cliente devolve elimina a proteção contra replay e torna toda a cerimônia decorativa.
- Desafio vale uma vez. Aqui a cerimônia é removida na leitura; uma segunda tentativa com o mesmo `ceremonyId` falha por construção.
- `ServerDomain` e `Origins` precisam bater com onde a página realmente roda. Configuração errada aqui produz falha de verificação genérica, e o instinto de "relaxar a validação" para destravar remove justamente a proteção contra phishing.
- Nunca use e-mail ou nome como `user.id`. Esse valor fica gravado no autenticador e pode aparecer na tela de seleção de contas; use um identificador opaco e estável, como o handle aleatório deste exemplo.
- O `userHandle` precisa ser estável por usuário. Se mudar entre registros, o autenticador trata a mesma pessoa como outra conta e cria credenciais duplicadas.
- Mande `excludeCredentials` no registro. Sem isso, o mesmo autenticador cria uma segunda passkey para a mesma conta, sem erro e sem utilidade.
- O contador de assinaturas detecta clonagem apenas quando o autenticador o implementa. Muitos devolvem sempre zero — nesse caso, a verificação não afirma nada, e tratar contador zerado como ataque gera falso positivo.
- `attestation: none` serve para a maioria dos casos. Exigir atestação só faz sentido quando a política precisa restringir modelos específicos de autenticador, e traz o custo de validar cadeias de certificado do fabricante.
- Conversão base64url ↔ `ArrayBuffer` é onde integrações WebAuthn mais quebram. O servidor fala base64url; `navigator.credentials` fala `ArrayBuffer`.
- Registre mais de uma passkey por conta, ou tenha um caminho de recuperação. Passkey presa a um único dispositivo perdido é conta perdida.

## Conteúdo complementar

Endpoints:

| Rota | Papel na cerimônia |
|---|---|
| `POST /passkey/register/begin` | Servidor gera o desafio e as opções de criação |
| `POST /passkey/register/complete` | Verifica a resposta e guarda a chave pública |
| `POST /passkey/login/begin` | Servidor gera o desafio de autenticação |
| `POST /passkey/login/complete` | Confere a assinatura com a chave pública guardada |
| `GET /passkey/credentials` | Mostra o que foi armazenado (apenas para o exemplo) |

As duas cerimônias:

```text
REGISTRO
  navegador  --- username ------------------>  servidor
  navegador  <-- challenge + rp + user ------  servidor
  autenticador: cria par de chaves, assina o desafio
  navegador  --- chave publica + assinatura ->  servidor
  servidor: confere origem, desafio e assinatura; guarda a chave publica

LOGIN
  navegador  --- (username opcional) -------->  servidor
  navegador  <-- challenge + allowCredentials-  servidor
  autenticador: destrava com biometria/PIN, assina o desafio
  navegador  --- assinatura ----------------->  servidor
  servidor: confere a assinatura com a chave publica guardada
```

O que o servidor guarda por credencial:

| Campo | Para quê |
|---|---|
| `credentialId` | Identificar a credencial no login |
| `publicKey` | Verificar a assinatura |
| `userHandle` | Ligar a credencial ao usuário sem expor o e-mail |
| `signCounter` | Detectar clonagem, quando o autenticador o implementa |
| `aaGuid` | Identificar o modelo do autenticador na lista de dispositivos |

Nenhum desses campos é secreto.

Por que passkey resiste a phishing e as alternativas não:

| Mecanismo | O usuário pode entregar ao atacante? | Preso ao domínio? |
|---|---|---|
| Senha | Sim | Não |
| OTP / código de SMS | Sim | Não |
| App autenticador (TOTP) | Sim | Não |
| Passkey | Não há o que entregar | Sim, por RP ID |

Em todos os casos, menos o último, existe um valor que a vítima pode digitar em um site falso. Com passkey, o autenticador simplesmente não assina para um domínio diferente.

Relação com os vizinhos da trilha: `Authentication/Auth` e `AdvancedAuthSystem` autenticam por senha e emitem JWT. Este projeto substitui a etapa de provar identidade; o que fazer com a sessão depois é assunto de `RefreshTokenRotationDemo`, e o que o usuário pode fazer é de `PolicyBasedAuthorizationDemo`.

## Referências e documentação complementar

- https://www.w3.org/TR/webauthn-3/
- https://fidoalliance.org/passkeys/
- https://github.com/passwordless-lib/fido2-net-lib
- https://developer.chrome.com/docs/devtools/webauthn
