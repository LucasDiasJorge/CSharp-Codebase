using SecurityAndAuthentication.Data;
using SecurityAndAuthentication.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SecurityAndAuthentication;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Configure services
        ConfigureServices(builder.Services);
        var app = builder.Build();
        // Configure the HTTP request pipeline
        Configure(app, app.Environment);
        // Run the application
        app.Run();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        
        // Registrar AuthService
        services.AddScoped<AuthService>();
        
        // Configurar JWT
        var jwtKey = "f7181daf86a42135a80611aee6b016fc1234567890abcdefghijklmnopqrstuvwxyz"; // 256-bit key
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "localhost:5150",
                    ValidAudience = "myfront.service.com",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
        
        // Configurar CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("UserAuthDatabase");
        });
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        
        // Usar CORS
        app.UseCors("AllowFrontend");
        
        // Usar autenticação e autorização
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        
        // Garantir que o banco seja criado
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }
}
