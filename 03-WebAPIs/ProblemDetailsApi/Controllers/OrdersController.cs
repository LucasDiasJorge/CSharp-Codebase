using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ProblemDetailsApi.Models;
using ProblemDetailsApi.Services;

namespace ProblemDetailsApi.Controllers;

/// <summary>
/// Endpoints que exercitam cada caminho de erro. Repare que nenhum deles monta um
/// ProblemDetails para falha de domínio: eles apenas deixam a exceção subir.
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>200, ou 404 quando o pedido não existe.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<OrderResponse> GetById(int id)
    {
        return Ok(_orderService.GetById(id));
    }

    /// <summary>
    /// 201, 400 (validação de formato, automática pelo <c>[ApiController]</c>) ou
    /// 422 (regra de negócio).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<OrderResponse> Create(CreateOrderRequest request)
    {
        OrderResponse order = _orderService.Create(request);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>200, 404 ou 409 quando o pedido já foi pago.</summary>
    [HttpPost("{id:int}/payments")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<OrderResponse> Pay(int id)
    {
        return Ok(_orderService.Pay(id));
    }

    /// <summary>
    /// Validação feita à mão, sem exceção. Alternativa legítima quando a regra é local
    /// ao endpoint e você quer acumular vários erros por campo.
    /// </summary>
    [HttpPost("quotes")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<OrderResponse> Quote([FromQuery] string? sku, [FromQuery] int quantity)
    {
        ModelStateDictionary errors = new ModelStateDictionary();

        if (string.IsNullOrWhiteSpace(sku))
        {
            errors.AddModelError(nameof(sku), "O SKU e obrigatorio.");
        }

        if (quantity <= 0)
        {
            errors.AddModelError(nameof(quantity), "A quantidade deve ser maior que zero.");
        }

        if (errors.ErrorCount > 0)
        {
            // ValidationProblem produz o mesmo formato do 400 automatico, com o
            // dicionario "errors" por campo.
            return ValidationProblem(errors);
        }

        return Ok(new OrderResponse(200, sku!, quantity, "cotado"));
    }

    /// <summary>Dispara uma falha não prevista para exercitar o handler de fallback.</summary>
    [HttpGet("boom")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult Boom()
    {
        _orderService.Explode();

        return Ok();
    }
}
