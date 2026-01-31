using CustomFilterApi.Filters;
using CustomFilterApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomFilterApi.Controllers;

/// <summary>
/// Controller de produtos demonstrando o uso do filtro em outro contexto.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo produto.
    /// Neste caso, o filtro é aplicado apenas nesta action específica.
    /// </summary>
    [HttpPost]
    [ServiceFilter(typeof(LogPropertyFilter))] // Filtro aplicado apenas nesta action
    public IActionResult CreateProduct([FromBody] ProductDto product)
    {
        _logger.LogInformation("Criando produto: {ProductName}", product.Name);

        return Created($"/api/products/1", new
        {
            Message = "Produto criado com sucesso",
            Product = new
            {
                product.Name,
                product.Price,
                product.Category
            },
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Atualiza um produto existente.
    /// Esta action também usa o filtro.
    /// </summary>
    [HttpPut("{id}")]
    [ServiceFilter(typeof(LogPropertyFilter))]
    public IActionResult UpdateProduct(int id, [FromBody] ProductDto product)
    {
        return Ok(new
        {
            Message = $"Produto {id} atualizado",
            Product = product
        });
    }

    /// <summary>
    /// Lista produtos. Esta action NÃO usa o filtro.
    /// </summary>
    [HttpGet]
    public IActionResult GetProducts()
    {
        return Ok(new[]
        {
            new { Id = 1, Name = "Laptop", Price = 1299.99, Category = "Eletrônicos" },
            new { Id = 2, Name = "Mouse", Price = 29.99, Category = "Acessórios" }
        });
    }
}
