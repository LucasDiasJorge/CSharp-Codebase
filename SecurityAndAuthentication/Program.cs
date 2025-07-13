using SecurityAndAuthentication.Data;
using Microsoft.EntityFrameworkCore;

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
