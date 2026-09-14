using ApiVersioningDemo.Models;
using ApiVersioningDemo.Services;
using ApiVersioningDemo.Versioning;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Controllers.V2;

/// <summary>
/// Mesma rota lógica da v1, outro contrato: <c>GET /api/v2/products</c>.
/// Duas classes de controller para a mesma rota é o padrão do versionamento por URL —
/// cada versão evolui sem condicionais espalhadas pelo código da anterior.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly CatalogStore _store;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(CatalogStore store, ILogger<ProductsController> logger)
    {
        _store = store;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<ProductV2>> Get()
    {
        _logger.LogInformation(
            "Produtos servidos pela versao {Versao}, selecionada por {Origem}.",
            RequestedVersionDescriber.Describe(HttpContext),
            "segmento de URL");

        return Ok(_store.GetProductsV2());
    }
}
