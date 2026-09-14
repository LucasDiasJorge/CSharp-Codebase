using ApiVersioningDemo.Models;
using ApiVersioningDemo.Services;
using ApiVersioningDemo.Versioning;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Controllers.V1;

/// <summary>
/// Versionamento fora da URL. A rota é sempre <c>/api/orders</c>; quem escolhe a versão
/// é o header <c>X-Api-Version</c> ou a query string <c>?api-version=</c>.
/// A URL permanece estável — e a versão deixa de ser visível para quem só olha o log
/// de acesso, que é o preço dessa abordagem.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
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
    public ActionResult<IReadOnlyList<OrderV1>> Get()
    {
        _logger.LogInformation("Pedidos servidos pela versao {Versao} na rota estavel /api/orders.", RequestedVersionDescriber.Describe(HttpContext));

        return Ok(_store.GetOrdersV1());
    }
}
