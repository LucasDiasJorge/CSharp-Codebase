using ClassToDTO.Db;
using ClassToDTO.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClassToDTO.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public OrderController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // ====================== LAZY LOADING ======================
    [HttpGet("lazy")]
    public async Task<IActionResult> GetOrdersLazy()
    {
        // Lazy Loading requires:
        // 1. Navigation properties must be VIRTUAL
        // 2. Microsoft.EntityFrameworkCore.Proxies package installed
        // 3. UseLazyLoadingProxies() enabled in DbContext
        
        var orders = await _dbContext.Orders.ToListAsync();

        // Customer is loaded automatically when accessed (if proxies are enabled)
        var orderDTOs = orders.Select(o => new OrderDTO
        {
            Id = o.Id,
            CustomerName = o.Customer.Name, // This triggers a separate DB query per order!
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount
        }).ToList();

        return Ok(orderDTOs);
    }
    
    // ====================== EAGER LOADING ======================
    [HttpGet("eager")]
    public async Task<IActionResult> GetOrdersEager()
    {
        // Eager Loading uses Include() to load related data in a single query
        var orders = await _dbContext.Orders
            .Include(o => o.Customer) // Explicitly include Customer
            .Select(o => new OrderDTO
            {
                Id = o.Id,
                CustomerName = o.Customer.Name, // Already loaded
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount
            })
            .ToListAsync();

        return Ok(orders);
    }
    
    // ====================== EXPLICIT LOADING ======================
    [HttpGet("explicit")]
    public async Task<IActionResult> GetOrdersExplicit()
    {
        // First load just the Orders without related data
        var orders = await _dbContext.Orders.ToListAsync();

        // Then explicitly load Customer for each order
        foreach (var order in orders)
        {
            await _dbContext.Entry(order)
                .Reference(o => o.Customer)
                .LoadAsync();
        }

        var orderDTOs = orders.Select(o => new OrderDTO
        {
            Id = o.Id,
            CustomerName = o.Customer.Name, // Now loaded
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount
        }).ToList();

        return Ok(orderDTOs);
    }
}