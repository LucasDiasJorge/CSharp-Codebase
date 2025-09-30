
using System.Text;
using Auth.Middlewares.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Auth;

public class Program
{
    public static void Main(string[] args)
    {
    // Builder central: ponto de entrada para configuração de serviços e pipeline HTTP.
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // ------------------------------------------------------------
    // REGISTRO DE SERVIÇOS (Dependency Injection Container)
    // ------------------------------------------------------------
    // Controllers tradicionais (API REST). Evitamos minimal endpoints aqui para fins didáticos.
    builder.Services.AddControllers();
    // Gera descrição de endpoints (metadata) usada por ferramentas como Swagger (AddEndpointsApiExplorer).
    builder.Services.AddEndpointsApiExplorer();
    // Extensão customizada (em Auth.Middlewares.Extensions) que registra autenticação JWT
    // incluindo esquema JwtBearer + parâmetros de validação de token (issuer, audience, key, etc.).
    builder.Services.AddJWTAuth(builder.Configuration);

    // Constrói a aplicação (após este ponto, a coleção de serviços é "congelada").
    WebApplication app = builder.Build();

    // ------------------------------------------------------------
    // MIDDLEWARE PIPELINE (Ordem IMPORTA)
    // ------------------------------------------------------------
    // Middleware customizado para lidar com cenários JWT (ex.: extra logging / tratamento unificado de falhas de token).
    app.UseJWTAuthMiddleware();
    // Redireciona automaticamente HTTP -> HTTPS garantindo transporte seguro em dev.
    app.UseHttpsRedirection();
    // Adiciona autenticação (produz principal/ClaimsIdentity quando token válido).
    app.UseAuthentication();
    // Autoriza acesso conforme políticas/roles/claims após autenticação.
    app.UseAuthorization();
    // Mapeia endpoints de controllers detectados via roteamento por atributos.
    app.MapControllers();

    // Inicia o servidor Kestrel e bloqueia a thread até shutdown.
    app.Run();
    }
}
