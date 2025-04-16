using Microsoft.AspNetCore.Mvc;
using SimpleWebAPI.Models;

namespace SimpleWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    // Temporary in-memory list for demonstration
    private static List<ProductModel> _products = new List<ProductModel>();

    [HttpGet]
    public IActionResult GetProducts()
    {
        // Create sample products if the list is empty (for demonstration)
        if (!_products.Any())
        {
            _products.Add(new ProductModel(1L, "Product 1", "Description 1", 1499.90m));
            _products.Add(new ProductModel(2L, "Product 2", "Description 2", 2499.90m));
        }

        // Return the list of products with 200 OK status
        return Ok(_products);
    }

    [HttpPost]
    public IActionResult CreateProduct([FromBody] ProductModel product)
    {
        // Basic validation
        if (product == null)
        {
            return BadRequest("Product data is null");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Generate a new ID (in a real app, this would be done by the database)
        var newId = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
        product.Id = newId;

        // Add the product to our list
        _products.Add(product);

        // Return 201 Created status with the new product and location header
        return CreatedAtAction(
            nameof(GetProducts), 
            new { id = product.Id }, 
            product);
    }
}