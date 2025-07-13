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
        
        // Garantir que o banco seja criado e criar usuário master
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        // Criar usuário master se não existir
        CreateMasterUserAsync(context).Wait();
    }
    
    private static async Task CreateMasterUserAsync(ApplicationDbContext context)
    {
        // Verificar se já existe um usuário admin
        var existingAdmin = await context.Users.AnyAsync(u => u.Role == "Admin");
        
        if (!existingAdmin)
        {
            // Permitir configuração via environment variables ou usar padrões
            var masterEmail = Environment.GetEnvironmentVariable("MASTER_USER_EMAIL") ?? "admin@system.com";
            var masterUsername = Environment.GetEnvironmentVariable("MASTER_USER_USERNAME") ?? "admin";
            var masterPassword = Environment.GetEnvironmentVariable("MASTER_USER_PASSWORD") ?? "Admin123!";
            
            var masterUser = new SecurityAndAuthentication.Models.User
            {
                Email = masterEmail,
                Username = masterUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(masterPassword),
                Role = "Admin",
                IsSubscribed = false,
                CreatedAt = DateTime.UtcNow
            };
            
            context.Users.Add(masterUser);
            await context.SaveChangesAsync();
            
            Console.WriteLine("=================================");
            Console.WriteLine("👤 USUÁRIO MASTER CRIADO:");
            Console.WriteLine($"📧 Email: {masterUser.Email}");
            Console.WriteLine($"👤 Username: {masterUser.Username}");
            Console.WriteLine($"🔑 Password: {masterPassword}");
            Console.WriteLine($"🛡️ Role: {masterUser.Role}");
            Console.WriteLine($"📅 Created: {masterUser.CreatedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine("=================================");
            Console.WriteLine("⚠️ RECOMENDAÇÕES DE SEGURANÇA:");
            Console.WriteLine("1. Altere a senha após o primeiro login");
            Console.WriteLine("2. Use o endpoint POST /api/auth/change-password");
            Console.WriteLine("3. Configure MASTER_USER_* environment variables");
            Console.WriteLine("=================================");
        }
        else
        {
            Console.WriteLine("✅ Usuário administrador já existe - não criando master user");
        }
    }
}
