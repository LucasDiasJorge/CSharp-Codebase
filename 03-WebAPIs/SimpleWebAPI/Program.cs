using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Json;
using System;
using Microsoft.OpenApi.Models;

namespace SimpleWebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        // Configuração do Serilog antes de criar o builder
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Nível mínimo geral
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Reduz logs da Microsoft
            .MinimumLevel.Override("System", LogEventLevel.Warning) // Reduz logs do System
            .Enrich.FromLogContext() // Adiciona contexto ao log
            .Enrich.WithProperty("Application", "SimpleWebAPI") // Propriedade fixa
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "logs/log-.txt", 
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                new CompactJsonFormatter(),
                path: "logs/log-json-.json",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Usar Serilog como provider de logging
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            // Configuração do Swagger/OpenAPI
            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "SimpleWebAPI",
                        Version = "v1",
                        Description = "Uma API simples com Swagger",
                        Contact = new OpenApiContact
                        {
                            Name = "Seu Nome",
                            Email = "seu.email@example.com"
                        }
                    }
                );
            });
                    
            builder.Services.AddControllers();

            var app = builder.Build();

            // Middleware de logging personalizado
            app.Use(async (context, next) => 
            {
                var start = DateTime.UtcNow;
                Log.Information("Starting request {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                
                await next();
                
                var duration = DateTime.UtcNow - start;
                Log.Information("Completed request {Method} {Path} in {Duration}ms", 
                    context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
            });
            
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SimpleWebAPI v1");
                    // Para acessar na raiz (opcional): c.RoutePrefix = string.Empty;
                });
                
                Log.Information("Swagger UI enabled for development environment");                Log.Information("OpenAPI/Swagger enabled for development environment");
            }
            
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            Log.Information("Application starting up");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}