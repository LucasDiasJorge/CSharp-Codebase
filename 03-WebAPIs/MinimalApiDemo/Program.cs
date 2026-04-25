//https://medium.com/@fradkilo700/monitoring-and-managing-microservices-with-actuator-in-net-core-a-comprehensive-guide-3b3c996883de

using Microsoft.EntityFrameworkCore;
using Steeltoe.Management.Endpoint;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        ConfigureServices(services);

        var app = builder.Build();

        app.UseCors("_myAllowAllOrigins"); 

        ConfigureEndpoints(app);

        app.Run();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(name: "_myAllowAllOrigins",
                policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
        });

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase("ProductsDB"));
        services.AddHealthChecks();
        services.AddControllers();
        services.AddAllActuators();
    }

    private static void ConfigureEndpoints(WebApplication app)
    {

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapAllActuators();
        });

        // Get All Products
        app.MapGet("/products", async (ApplicationDbContext db) => await db.Products.ToListAsync());

        // Get Product by ID
        app.MapGet("/products/{id}", async (int id, ApplicationDbContext db) =>
            await db.Products.FindAsync(id) is Product product ? Results.Ok(product) : Results.NotFound());

        // Create a New Product
        app.MapPost("/products", async (Product product, ApplicationDbContext db) =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/products/{product.Id}", product);
        });

        // Update a Product
        app.MapPut("/products/{id}", async (int id, Product input, ApplicationDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null) return Results.NotFound();

            product.Name = input.Name;
            product.Price = input.Price;

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Delete a Product
        app.MapDelete("/products/{id}", async (int id, ApplicationDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null) return Results.NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
