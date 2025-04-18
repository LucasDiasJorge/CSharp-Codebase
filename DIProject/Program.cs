using DIProject;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Registering IMyService as a singleton ensures that the same instance of MyService is shared across the application.
builder.Services.AddSingleton<IMyService, MyService>();  // Singleton

// Registering IMyService as a transient ensures a creation of a new instance of MyService every time that is requested
//builder.Services.AddTransient<IMyService, MyService>();   // Transient

// Registering IMyService as a scoped ensures a that the same instance of MyService once per request
//builder.Services.AddScoped<IMyService, MyService>();    // Scoped

var app = builder.Build();

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("First Middleware");
    await next();
});

app.Use(async (context, next) =>
{
    var myService = context.RequestServices.GetRequiredService<IMyService>();
    myService.LogCreation("Second Middleware");
    await next();
});

app.MapGet("/", (IMyService myService) =>
{
    myService.LogCreation("Root");
    return Results.Ok("Check the console for service creation logs.");
});

app.Run();