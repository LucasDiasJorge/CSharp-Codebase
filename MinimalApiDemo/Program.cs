using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseInMemoryDatabase("ProductsDB"));

var app = builder.Build();

// Register services

// CRUD Endpoints

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

app.Run();
