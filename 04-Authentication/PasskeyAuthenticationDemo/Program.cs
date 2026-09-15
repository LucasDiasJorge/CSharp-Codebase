using PasskeyAuthenticationDemo.Credentials;
using PasskeyAuthenticationDemo.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CredentialStore>();

builder.Services.AddFido2(options =>
{
    // ServerDomain e o RP ID: o dominio ao qual a passkey fica presa. O autenticador se
    // recusa a assinar para qualquer outro — e ISSO que torna passkey resistente a
    // phishing. Um site clone em outro dominio nao consegue uma assinatura valida nem
    // com o usuario colaborando.
    options.ServerDomain = "localhost";
    options.ServerName = "PasskeyAuthenticationDemo";

    // A origem completa tambem e conferida, contra o campo origin do clientDataJSON.
    options.Origins = new HashSet<string> { "http://localhost:5078" };

    options.TimestampDriftTolerance = 300000;
});

WebApplication app = builder.Build();

// Pagina com navigator.credentials: WebAuthn so existe dentro do navegador.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPasskeyEndpoints();

app.Run();
