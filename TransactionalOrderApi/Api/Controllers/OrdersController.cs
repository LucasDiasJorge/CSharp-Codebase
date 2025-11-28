using Microsoft.AspNetCore.Mvc;
using TransactionalOrderApi.Application.Contracts.Requests;
using TransactionalOrderApi.Application.Services;

namespace TransactionalOrderApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await _orderService.PlaceOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var response = await _orderService.GetOrderAsync(id, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
