using Fido2NetLib;
using Fido2NetLib.Objects;
using PasskeyAuthenticationDemo.Credentials;

namespace PasskeyAuthenticationDemo.Endpoints;

/// <summary>
/// As duas cerimônias do WebAuthn. Cada uma tem dois passos: o servidor emite um desafio
/// e depois verifica a resposta assinada do autenticador.
/// </summary>
public static class PasskeyEndpoints
{
    public static void MapPasskeyEndpoints(this WebApplication app)
    {
        MapRegistration(app);
        MapAssertion(app);
        MapInspection(app);
    }

    private static void MapRegistration(WebApplication app)
    {
        // Passo 1 do registro: o servidor gera o desafio e diz que tipo de credencial
        // aceita. Nada foi criado ainda.
        app.MapPost("/passkey/register/begin", (RegisterBeginRequest request, IFido2 fido2, CredentialStore store, ILoggerFactory loggerFactory) =>
        {
            ILogger logger = loggerFactory.CreateLogger("Passkey.Register");

            byte[] userHandle = store.GetOrCreateUserHandle(request.Username);

            // Credenciais já registradas entram em excludeCredentials: é o que impede o
            // mesmo autenticador de criar uma segunda passkey para a mesma conta.
            List<PublicKeyCredentialDescriptor> existing = new List<PublicKeyCredentialDescriptor>();
            foreach (StoredCredential credential in store.FindByUsername(request.Username))
            {
                existing.Add(new PublicKeyCredentialDescriptor(credential.CredentialId));
            }

            CredentialCreateOptions options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = new Fido2User
                {
                    Id = userHandle,
                    Name = request.Username,
                    DisplayName = request.Username
                },
                ExcludeCredentials = existing,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    // Discoverable credential: a passkey fica guardada no autenticador com
                    // o handle do usuario dentro, o que permite login sem digitar usuario.
                    ResidentKey = ResidentKeyRequirement.Preferred,

                    // Exige biometria ou PIN. E o segundo fator embutido: posse do
                    // dispositivo mais algo que o usuario e ou sabe.
                    UserVerification = UserVerificationRequirement.Preferred
                },

                // "None" dispensa a cadeia de atestacao do fabricante. Suficiente para a
                // maioria dos casos; exigir atestacao so faz sentido quando a politica
                // precisa restringir modelos especificos de autenticador.
                AttestationPreference = AttestationConveyancePreference.None
            });

            string ceremonyId = store.StoreRegistrationCeremony(options, request.Username);
            logger.LogInformation("Registro iniciado para {Usuario}, cerimonia {Cerimonia}.", request.Username, ceremonyId);

            return Results.Ok(new { ceremonyId, options });
        });

        // Passo 2 do registro: o autenticador criou o par de chaves e assinou o desafio.
        // O servidor verifica e guarda apenas a chave publica.
        app.MapPost("/passkey/register/complete", async (RegisterCompleteRequest request, IFido2 fido2, CredentialStore store, ILoggerFactory loggerFactory) =>
        {
            ILogger logger = loggerFactory.CreateLogger("Passkey.Register");

            PendingCeremony? ceremony = store.TakeCeremony(request.CeremonyId);
            if (ceremony?.CreationOptionsJson is null)
            {
                return Results.BadRequest(new { error = "Cerimonia desconhecida ou ja consumida." });
            }

            CredentialCreateOptions originalOptions = CredentialCreateOptions.FromJson(ceremony.CreationOptionsJson);

            try
            {
                RegisteredPublicKeyCredential credential = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = request.Response,

                    // O desafio conferido e o que o SERVIDOR emitiu, recuperado do
                    // armazenamento — nunca o que o cliente devolveu.
                    OriginalOptions = originalOptions,
                    IsCredentialIdUniqueToUserCallback = (args, _) =>
                        Task.FromResult(store.IsCredentialIdUnique(args.CredentialId))
                });

                store.Add(new StoredCredential(
                    credential.Id,
                    credential.PublicKey,
                    originalOptions.User.Id,
                    credential.SignCount,
                    ceremony.Username!,
                    credential.AaGuid));

                logger.LogInformation("Passkey registrada para {Usuario}.", ceremony.Username);

                return Results.Ok(new
                {
                    registered = true,
                    username = ceremony.Username,
                    credentialId = Convert.ToBase64String(credential.Id),
                    signCount = credential.SignCount
                });
            }
            catch (Fido2VerificationException ex)
            {
                logger.LogWarning("Falha na verificacao do registro: {Mensagem}", ex.Message);

                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }

    private static void MapAssertion(WebApplication app)
    {
        // Passo 1 do login: desafio novo. Sem usuario informado, o navegador escolhe
        // entre as passkeys descobriveis que tiver para este dominio.
        app.MapPost("/passkey/login/begin", (LoginBeginRequest request, IFido2 fido2, CredentialStore store) =>
        {
            List<PublicKeyCredentialDescriptor> allowed = new List<PublicKeyCredentialDescriptor>();

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                foreach (StoredCredential credential in store.FindByUsername(request.Username))
                {
                    allowed.Add(new PublicKeyCredentialDescriptor(credential.CredentialId));
                }

                if (allowed.Count == 0)
                {
                    return Results.NotFound(new { error = "Nenhuma passkey registrada para este usuario." });
                }
            }

            AssertionOptions options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                // Lista vazia = login sem usuario (usernameless), so possivel com
                // credenciais descobriveis.
                AllowedCredentials = allowed,
                UserVerification = UserVerificationRequirement.Preferred
            });

            string ceremonyId = store.StoreAssertionCeremony(options, request.Username);

            return Results.Ok(new { ceremonyId, options });
        });

        // Passo 2 do login: verifica a assinatura com a chave publica guardada.
        app.MapPost("/passkey/login/complete", async (LoginCompleteRequest request, IFido2 fido2, CredentialStore store, ILoggerFactory loggerFactory) =>
        {
            ILogger logger = loggerFactory.CreateLogger("Passkey.Login");

            PendingCeremony? ceremony = store.TakeCeremony(request.CeremonyId);
            if (ceremony?.AssertionOptionsJson is null)
            {
                return Results.BadRequest(new { error = "Cerimonia desconhecida ou ja consumida." });
            }

            StoredCredential? stored = store.FindByCredentialId(request.Response.RawId);
            if (stored is null)
            {
                return Results.BadRequest(new { error = "Credencial desconhecida." });
            }

            AssertionOptions originalOptions = AssertionOptions.FromJson(ceremony.AssertionOptionsJson);

            try
            {
                VerifyAssertionResult result = await fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = request.Response,
                    OriginalOptions = originalOptions,
                    StoredPublicKey = stored.PublicKey,
                    StoredSignatureCounter = stored.SignCounter,
                    IsUserHandleOwnerOfCredentialIdCallback = (args, _) =>
                    {
                        IReadOnlyList<StoredCredential> owned = store.FindByUserHandle(args.UserHandle);
                        foreach (StoredCredential credential in owned)
                        {
                            if (credential.CredentialId.AsSpan().SequenceEqual(args.CredentialId))
                            {
                                return Task.FromResult(true);
                            }
                        }

                        return Task.FromResult(false);
                    }
                });

                // Contador sempre para frente. A biblioteca ja rejeita retrocesso; guardar
                // o novo valor e o que mantem a verificacao valendo na proxima vez.
                stored.UpdateCounter(result.SignCount);

                logger.LogInformation("Login por passkey concluido para {Usuario}.", stored.Username);

                return Results.Ok(new
                {
                    authenticated = true,
                    username = stored.Username,
                    signCount = result.SignCount
                });
            }
            catch (Fido2VerificationException ex)
            {
                logger.LogWarning("Falha na verificacao do login: {Mensagem}", ex.Message);

                return Results.Unauthorized();
            }
        });
    }

    private static void MapInspection(WebApplication app)
    {
        // Mostra o que o servidor realmente guarda. So existe para o exemplo — e o ponto
        // e justamente que nao ha nada de secreto aqui.
        app.MapGet("/passkey/credentials", (CredentialStore store) =>
        {
            List<object> credentials = new List<object>();
            foreach (StoredCredential credential in store.GetAll())
            {
                credentials.Add(new
                {
                    username = credential.Username,
                    credentialId = Convert.ToBase64String(credential.CredentialId),
                    publicKeyBytes = credential.PublicKey.Length,
                    signCounter = credential.SignCounter,
                    aaGuid = credential.AaGuid,
                    createdAt = credential.CreatedAt,
                    lastUsedAt = credential.LastUsedAt
                });
            }

            return Results.Ok(credentials);
        });
    }
}

public sealed record RegisterBeginRequest(string Username);

public sealed record RegisterCompleteRequest(string CeremonyId, AuthenticatorAttestationRawResponse Response);

public sealed record LoginBeginRequest(string? Username);

public sealed record LoginCompleteRequest(string CeremonyId, AuthenticatorAssertionRawResponse Response);
