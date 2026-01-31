using GoodOrderApi.Api.DTOs;
using GoodOrderApi.Application.Services;
using GoodOrderApi.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace GoodOrderApi.Api.Controllers;

// ✅ REGRA 1: Um nível de indentação
// ✅ REGRA 2: Sem ELSE - usando early returns e ternários
// ✅ REGRA 7: Controller pequeno e focado

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderApplicationService _orderService;
    private readonly ReportService _reportService;

    public OrdersController(OrderApplicationService orderService, ReportService reportService)
    {
        _orderService = orderService;
        _reportService = reportService;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var command = OrderMapper.ToCommand(request);
            var result = _orderService.CreateOrder(command);
            
            if (result.IsFailure)
                return BadRequest(new ErrorResponse { Message = result.Error });

            var response = OrderMapper.ToResponse(result.Value!);
            return CreatedAtAction(nameof(GetOrder), new { id = response.Id }, response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        var order = _orderService.GetOrderById(id);
        
        if (order is null)
            return NotFound(new ErrorResponse { Message = "Order not found" });

        return Ok(OrderMapper.ToResponse(order));
    }

    [HttpGet]
    public IActionResult GetAllOrders([FromQuery] string? status)
    {
        var orders = string.IsNullOrEmpty(status)
            ? _orderService.GetAllOrders()
            : _orderService.GetOrdersByStatus(status);

        var response = orders.Select(OrderMapper.ToResponse).ToList();
        return Ok(response);
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var result = _orderService.UpdateOrderStatus(id, request.Status);
        
        if (result.IsFailure)
            return BadRequest(new ErrorResponse { Message = result.Error });

        return Ok(new SuccessResponse { Message = result.Message });
    }

    [HttpPost("{id}/payment")]
    public IActionResult ProcessPayment(int id, [FromBody] PaymentRequest request)
    {
        var result = _orderService.ProcessPayment(id, request.PaymentMethod);
        
        if (result.IsFailure)
            return BadRequest(new ErrorResponse { Message = result.Error });

        return Ok(new SuccessResponse { Message = result.Message });
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        var products = _orderService.GetAllProducts();
        var response = products.Select(OrderMapper.ToResponse).ToList();
        return Ok(response);
    }

    [HttpGet("report")]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var report = _reportService.GenerateReport(startDate, endDate);
        return Ok(OrderMapper.ToResponse(report));
    }
}
