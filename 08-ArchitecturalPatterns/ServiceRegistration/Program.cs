using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register pre-execution service
builder.Services.AddTransient<IPreExecutionService, PreExecutionService>();

// Register post-execution service
builder.Services.AddTransient<IPostExecutionService, PostExecutionService>();

// Registering IMyService as a singleton ensures that the same instance of MyService is shared across the application.
//builder.Services.AddSingleton<IMyService, MyService>();  // Singleton

// Registering IMyService as a transient ensures a creation of a new instance of MyService every time that is requested
builder.Services.AddTransient<IMyService, MyService>();   // Transient

// Registering IMyService as a scoped ensures a that the same instance of MyService once per request
//builder.Services.AddScoped<IMyService, MyService>();    // Scoped

var app = builder.Build();

// Middleware that runs before the endpoint
app.Use(async (context, next) =>
{
    var preService = context.RequestServices.GetRequiredService<IPreExecutionService>();
    preService.Execute("Running BEFORE endpoint execution");
    await next();
});

app.MapGet("/", (IMyService myService) =>
{
    myService.LogCreation("Root");
    return Results.Ok("Check the console for execution logs.");
});

// Middleware that runs after the endpoint
app.Use(async (context, next) =>
{
    // First let the request continue (important for error cases)
    await next();
    
    // Only execute after if this is our target endpoint
    if (context.Request.Path == "/")
    {
        var postService = context.RequestServices.GetRequiredService<IPostExecutionService>();
        postService.Execute("Running AFTER endpoint execution");
    }
});

app.Run();