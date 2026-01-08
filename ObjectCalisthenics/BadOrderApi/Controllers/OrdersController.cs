using BadOrderApi.Services;
using BadOrderApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BadOrderApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        var (success, message, order) = _orderService.CreateOrder(request);

        if (success)
        {
            var response = new OrderResponse
            {
                Id = order!.Id,
                CustomerName = order.CustName,
                Status = order.Status,
                Total = order.TotalAmt,
                CreatedAt = order.CreatedDt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductName = i.ProdName,
                    Quantity = i.Qty,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
        }
        else
        {
            return BadRequest(new { Message = message });
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        var order = _orderService.GetOrderById(id);

        if (order != null)
        {
            var response = new OrderResponse
            {
                Id = order.Id,
                CustomerName = order.CustName,
                Status = order.Status,
                Total = order.TotalAmt,
                CreatedAt = order.CreatedDt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductName = i.ProdName,
                    Quantity = i.Qty,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };

            return Ok(response);
        }
        else
        {
            return NotFound(new { Message = "Order not found" });
        }
    }

    [HttpGet]
    public IActionResult GetAllOrders([FromQuery] string? status)
    {
        var orders = string.IsNullOrEmpty(status) 
            ? _orderService.GetAllOrders() 
            : _orderService.GetOrdersByStatus(status);

        var response = orders.Select(order => new OrderResponse
        {
            Id = order.Id,
            CustomerName = order.CustName,
            Status = order.Status,
            Total = order.TotalAmt,
            CreatedAt = order.CreatedDt,
            Items = order.Items.Select(i => new OrderItemResponse
            {
                ProductName = i.ProdName,
                Quantity = i.Qty,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        }).ToList();

        return Ok(response);
    }

    [HttpPut("{id}/status")]
    public IActionResult UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var (success, message) = _orderService.UpdateOrderStatus(id, request.Status);

        if (success)
        {
            return Ok(new { Message = message });
        }
        else
        {
            return BadRequest(new { Message = message });
        }
    }

    [HttpPost("{id}/payment")]
    public IActionResult ProcessPayment(int id, [FromBody] PaymentRequest request)
    {
        var (success, message) = _orderService.ProcessPayment(id, request.PaymentMethod);

        if (success)
        {
            return Ok(new { Message = message });
        }
        else
        {
            return BadRequest(new { Message = message });
        }
    }

    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        return Ok(_orderService.GetAllProducts());
    }

    [HttpGet("report")]
    public IActionResult GetReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        return Ok(_orderService.GenerateReport(startDate, endDate));
    }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class PaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
}
