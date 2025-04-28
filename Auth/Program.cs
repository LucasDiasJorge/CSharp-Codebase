
using System.Text;
using Auth.Middlewares.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Auth;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddJWTAuth(builder.Configuration);
        
        var app = builder.Build();

        app.UseJWTAuthMiddleware();
        app.UseHttpsRedirection();
        app.UseAuthentication(); // Enable JWT authentication
        app.UseAuthorization();  // Enable authorization
        app.MapControllers();   // Map controller routes

        app.Run(); // Run the app
    }
}
