using ApiVersioningDemo.Models;
using ApiVersioningDemo.Services;
using ApiVersioningDemo.Versioning;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace ApiVersioningDemo.Controllers.V1;

/// <summary>
/// Versionamento por segmento de URL. A versão faz parte do recurso:
/// <c>GET /api/v1/products</c>.
/// Marcada como <c>Deprecated</c>: continua funcionando, mas anuncia a própria
/// aposentadoria nos headers de toda resposta.
/// </summary>
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
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
    public ActionResult<IReadOnlyList<ProductV1>> Get()
    {
        _logger.LogInformation(
            "Produtos servidos pela versao {Versao} (depreciada), selecionada por {Origem}.",
            RequestedVersionDescriber.Describe(HttpContext),
            "segmento de URL");

        return Ok(_store.GetProductsV1());
    }
}
