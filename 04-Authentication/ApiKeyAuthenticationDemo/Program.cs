using ApiKeyAuthenticationDemo.Authentication;
using ApiKeyAuthenticationDemo.Keys;
using Microsoft.AspNetCore.Authentication;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ApiKeyStore>();

builder.Services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyDefaults.AuthenticationScheme,
        configureOptions: null);

builder.Services.AddAuthorization(options =>
{
    // Escopo virou claim no handler; aqui ele vira policy. O endpoint declara a
    // permissao de que precisa, sem saber que a credencial era uma chave de API.
    options.AddPolicy("data:read", policy =>
        policy.RequireClaim(ApiKeyAuthenticationHandler.ScopeClaim, "data:read"));

    options.AddPolicy("data:write", policy =>
        policy.RequireClaim(ApiKeyAuthenticationHandler.ScopeClaim, "data:write"));
});

WebApplication app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// --- Administracao das chaves -------------------------------------------------------
// Em producao estes endpoints estariam protegidos e separados; aqui ficam abertos para
// o exemplo ser executavel por curl.

app.MapPost("/admin/keys", (IssueKeyRequest request, ApiKeyStore store) =>
{
    ApiKeyIssueResult issued = store.Issue(request.Owner, request.Scopes);

    return Results.Ok(new
    {
        keyId = issued.Key.Id,

        // Unica vez que o valor em claro aparece. Nao ha endpoint para reexibi-lo,
        // porque o servidor guarda so o hash.
        apiKey = issued.PlaintextToken,
        scopes = issued.Key.Scopes,
        aviso = "Guarde agora: esta chave nao sera exibida de novo."
    });
});

app.MapPost("/admin/keys/{keyId}/rotate", (string keyId, ApiKeyStore store) =>
{
    ApiKeyIssueResult? successor = store.Rotate(keyId);

    return successor is null
        ? Results.NotFound(new { error = "Chave desconhecida ou revogada." })
        : Results.Ok(new
        {
            newKeyId = successor.Key.Id,
            apiKey = successor.PlaintextToken,
            aviso = "A chave anterior continua valendo por 10 minutos."
        });
});

app.MapDelete("/admin/keys/{keyId}", (string keyId, ApiKeyStore store) =>
{
    return store.Revoke(keyId)
        ? Results.Ok(new { revoked = keyId })
        : Results.NotFound();
});

app.MapGet("/admin/keys", (ApiKeyStore store) =>
{
    List<object> keys = new List<object>();
    foreach (ApiKey key in store.GetAll())
    {
        keys.Add(new
        {
            id = key.Id,
            owner = key.Owner,
            scopes = key.Scopes,
            createdAt = key.CreatedAt,

            // lastUsedAt e o que permite achar chave esquecida, criada para um teste e
            // nunca revogada. Chave sem uso ha meses e risco sem contrapartida.
            lastUsedAt = key.LastUsedAt,
            revokedAt = key.RevokedAt,
            expiresAt = key.ExpiresAt,
            rotatedTo = key.RotatedToKeyId

            // O hash do segredo nao aparece aqui, nem deveria.
        });
    }

    return Results.Ok(keys);
});

// --- Recursos protegidos ------------------------------------------------------------

app.MapGet("/data/public", () => Results.Ok(new { message = "Aberto, sem chave." }));

app.MapGet("/data/read", (HttpContext context) => Results.Ok(new
{
    message = "Leitura autorizada.",
    keyId = context.User.FindFirst(ApiKeyAuthenticationHandler.KeyIdClaim)?.Value,
    owner = context.User.Identity?.Name
})).RequireAuthorization("data:read");

app.MapPost("/data/write", () => Results.Ok(new { message = "Escrita autorizada." }))
    .RequireAuthorization("data:write");

app.Run();

internal sealed record IssueKeyRequest(string Owner, string[] Scopes);
