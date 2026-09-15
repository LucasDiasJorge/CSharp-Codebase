using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RefreshTokenRotationDemo.Tokens;
using RefreshTokenRotationDemo.Users;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Chave de desenvolvimento. Em producao viria de um cofre (Key Vault, Secrets Manager),
// nunca do appsettings versionado: quem tem a chave de assinatura emite tokens validos.
JwtOptions jwtOptions = new JwtOptions(
    issuer: "RefreshTokenRotationDemo",
    audience: "RefreshTokenRotationDemo.Clients",
    signingKey: builder.Configuration["Jwt:SigningKey"] ?? "chave-de-desenvolvimento-com-pelo-menos-32-bytes!!");

builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<RefreshTokenStore>();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,

            // Padrao do framework e 5 minutos de tolerancia no relogio. Com access token
            // de 60 segundos isso tornaria a expiracao invisivel no exemplo — e, em
            // producao, estenderia silenciosamente a vida de todo token.
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
