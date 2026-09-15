using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PolicyBasedAuthorizationDemo.Authorization;
using PolicyBasedAuthorizationDemo.Authorization.Handlers;
using PolicyBasedAuthorizationDemo.Authorization.Requirements;
using PolicyBasedAuthorizationDemo.Documents;
using PolicyBasedAuthorizationDemo.Users;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DocumentStore>();
builder.Services.AddSingleton<DemoTokenIssuer>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = DemoTokenIssuer.Issuer,
            ValidateAudience = true,
            ValidAudience = DemoTokenIssuer.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DemoTokenIssuer.SigningKey)),
            ValidateLifetime = true,

            // Sem isto, o handler renomeia "sub" para o ClaimTypes.NameIdentifier longo
            // e User.FindFirst("sub") volta nulo — quebrando o handler de dono do
            // documento sem nenhum erro visivel.
            NameClaimType = "unique_name",
            RoleClaimType = ClaimTypes.Role
        };

        options.MapInboundClaims = false;
    });

// Os handlers de requirement sao servicos comuns: registrados no container e resolvidos
// pelo motor de autorizacao. Dois handlers para DocumentEditRequirement significa dois
// caminhos alternativos (OU) de satisfaze-la.
builder.Services.AddSingleton<IAuthorizationHandler, MinimumClearanceHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DocumentOwnerHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, DocumentAdminHandler>();

builder.Services.AddAuthorization(options =>
{
    // Requirement proprio: a regra fica no handler, nao no atributo.
    options.AddPolicy(Policies.ConfidentialAccess, policy =>
        policy.Requirements.Add(new MinimumClearanceRequirement(3)));

    // Caso mais simples: claim com valor exato.
    options.AddPolicy(Policies.EngineeringOnly, policy =>
        policy.RequireClaim(DemoTokenIssuer.DepartmentClaim, "engineering"));

    // Regra inline. Boa para condicao pontual; ruim para regra que precise de servico
    // injetado ou de teste isolado — nesses casos, vale um requirement de verdade.
    options.AddPolicy(Policies.WeekdayOnly, policy =>
        policy.RequireAssertion(context =>
            DateTime.UtcNow.DayOfWeek != DayOfWeek.Saturday &&
            DateTime.UtcNow.DayOfWeek != DayOfWeek.Sunday));

    // Duas exigencias na mesma policy: ambas precisam ser satisfeitas.
    options.AddPolicy(Policies.SeniorEngineering, policy =>
    {
        policy.RequireClaim(DemoTokenIssuer.DepartmentClaim, "engineering");
        policy.Requirements.Add(new MinimumClearanceRequirement(5));
    });
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Emissao de token so para exercitar as policies — autenticacao nao e o assunto aqui.
app.MapPost("/dev/token", (DevTokenRequest request, DemoTokenIssuer issuer) =>
{
    string? token = issuer.IssueFor(request.User);

    return token is null
        ? Results.NotFound(new { error = "Usuario desconhecido.", known = issuer.KnownUsers })
        : Results.Ok(new { token });
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

internal sealed record DevTokenRequest(string User);
