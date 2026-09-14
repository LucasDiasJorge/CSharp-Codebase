using ApiVersioningDemo.Models;
using ApiVersioningDemo.Services;
using ApiVersioningDemo.Versioning;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Controllers.V2;

/// <summary>
/// v2 de pedidos na mesma rota da v1. Sem o header ou a query string, o pipeline cai na
/// versão padrão (1.0) por causa de <c>AssumeDefaultVersionWhenUnspecified</c>.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly CatalogStore _store;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(CatalogStore store, ILogger<OrdersController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<OrderV2>> Get()
    {
        _logger.LogInformation("Pedidos servidos pela versao {Versao} na rota estavel /api/orders.", RequestedVersionDescriber.Describe(HttpContext));

        return Ok(_store.GetOrdersV2());
    }
}
