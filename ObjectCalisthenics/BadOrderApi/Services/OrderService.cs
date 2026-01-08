using BadOrderApi.Models;
using BadOrderApi.DTOs;

namespace BadOrderApi.Services;

// ❌ VIOLAÇÃO: Classe muito grande (Regra 7)
// ❌ VIOLAÇÃO: Múltiplos níveis de indentação (Regra 1)
// ❌ VIOLAÇÃO: Uso excessivo de ELSE (Regra 2)
// ❌ VIOLAÇÃO: Múltiplos pontos por linha (Regra 5)
public class OrderService
{
    private static readonly List<Order> _orders = new();
    private static readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop", Desc = "High performance laptop", Price = 1299.99m, StockQty = 50, Cat = "Electronics", IsActive = true },
        new Product { Id = 2, Name = "Mouse", Desc = "Wireless mouse", Price = 29.99m, StockQty = 200, Cat = "Electronics", IsActive = true },
        new Product { Id = 3, Name = "Keyboard", Desc = "Mechanical keyboard", Price = 79.99m, StockQty = 100, Cat = "Electronics", IsActive = true },
        new Product { Id = 4, Name = "Monitor", Desc = "27 inch 4K monitor", Price = 499.99m, StockQty = 30, Cat = "Electronics", IsActive = true },
        new Product { Id = 5, Name = "Headphones", Desc = "Noise cancelling headphones", Price = 199.99m, StockQty = 75, Cat = "Electronics", IsActive = true },
    };

    private static int _nextOrderId = 1;

    // ❌ VIOLAÇÃO GRAVE: Múltiplos níveis de indentação (Regra 1)
    // ❌ VIOLAÇÃO: Uso excessivo de ELSE (Regra 2)
    // ❌ VIOLAÇÃO: Múltiplos pontos por linha (Regra 5)
    public (bool Success, string Message, Order? Order) CreateOrder(CreateOrderRequest request)
    {
        // Nível 1 - validação do request
        if (request != null)
        {
            // Nível 2 - validação do nome
            if (!string.IsNullOrEmpty(request.CustomerName))
            {
                // Nível 3 - validação do email
                if (!string.IsNullOrEmpty(request.CustomerEmail))
                {
                    // Nível 4 - validação dos itens
                    if (request.Items != null && request.Items.Count > 0)
                    {
                        var order = new Order
                        {
                            Id = _nextOrderId++,
                            CustName = request.CustomerName,
                            CustEmail = request.CustomerEmail,
                            CustPhone = request.CustomerPhone,
                            CustAddr = request.CustomerAddress,
                            CustCity = request.CustomerCity,
                            CustZip = request.CustomerZip,
                            PaymentMethod = request.PaymentMethod,
                            CreatedDt = DateTime.UtcNow,
                            Status = "Pending"
                        };

                        decimal totalAmount = 0;

                        // Nível 5 - iteração nos itens
                        foreach (var item in request.Items)
                        {
                            // ❌ Múltiplos pontos por linha (Regra 5)
                            var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                            
                            // Nível 6 - verificação do produto
                            if (product != null)
                            {
                                // Nível 7 - verificação do estoque
                                if (product.StockQty >= item.Quantity)
                                {
                                    // Nível 8 - verificação se produto ativo
                                    if (product.IsActive)
                                    {
                                        var orderItem = new OrderItem
                                        {
                                            Id = order.Items.Count + 1,
                                            OrderId = order.Id,
                                            ProdName = product.Name,
                                            ProdDesc = product.Desc,
                                            Qty = item.Quantity,
                                            UnitPrice = product.Price,
                                            TotalPrice = product.Price * item.Quantity
                                        };

                                        order.Items.Add(orderItem);
                                        totalAmount += orderItem.TotalPrice;
                                        product.StockQty -= item.Quantity;
                                    }
                                    else
                                    {
                                        return (false, $"Product {product.Name} is not active", null);
                                    }
                                }
                                else
                                {
                                    return (false, $"Insufficient stock for {product.Name}", null);
                                }
                            }
                            else
                            {
                                return (false, $"Product with ID {item.ProductId} not found", null);
                            }
                        }

                        // Aplicar desconto
                        if (!string.IsNullOrEmpty(request.DiscountCode))
                        {
                            if (request.DiscountCode == "SAVE10")
                            {
                                order.DiscCode = request.DiscountCode;
                                order.DiscAmt = totalAmount * 0.10m;
                                totalAmount -= order.DiscAmt;
                            }
                            else if (request.DiscountCode == "SAVE20")
                            {
                                order.DiscCode = request.DiscountCode;
                                order.DiscAmt = totalAmount * 0.20m;
                                totalAmount -= order.DiscAmt;
                            }
                            else if (request.DiscountCode == "SAVE50")
                            {
                                order.DiscCode = request.DiscountCode;
                                order.DiscAmt = totalAmount * 0.50m;
                                totalAmount -= order.DiscAmt;
                            }
                            else
                            {
                                // Código inválido, não aplica desconto
                                order.DiscCode = "";
                                order.DiscAmt = 0;
                            }
                        }
                        else
                        {
                            order.DiscCode = "";
                            order.DiscAmt = 0;
                        }

                        order.TotalAmt = totalAmount;
                        _orders.Add(order);

                        return (true, "Order created successfully", order);
                    }
                    else
                    {
                        return (false, "Order must have at least one item", null);
                    }
                }
                else
                {
                    return (false, "Customer email is required", null);
                }
            }
            else
            {
                return (false, "Customer name is required", null);
            }
        }
        else
        {
            return (false, "Request cannot be null", null);
        }
    }

    // ❌ Outro método com múltiplos níveis e else
    public (bool Success, string Message) UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        
        if (order != null)
        {
            if (newStatus == "Confirmed")
            {
                if (order.Status == "Pending")
                {
                    order.Status = newStatus;
                    return (true, "Order confirmed");
                }
                else
                {
                    return (false, "Order must be pending to confirm");
                }
            }
            else if (newStatus == "Shipped")
            {
                if (order.Status == "Confirmed")
                {
                    if (order.IsPaid)
                    {
                        order.Status = newStatus;
                        order.ShippedDt = DateTime.UtcNow;
                        return (true, "Order shipped");
                    }
                    else
                    {
                        return (false, "Order must be paid before shipping");
                    }
                }
                else
                {
                    return (false, "Order must be confirmed before shipping");
                }
            }
            else if (newStatus == "Delivered")
            {
                if (order.Status == "Shipped")
                {
                    order.Status = newStatus;
                    return (true, "Order delivered");
                }
                else
                {
                    return (false, "Order must be shipped before delivery");
                }
            }
            else if (newStatus == "Cancelled")
            {
                if (order.Status != "Delivered" && order.Status != "Shipped")
                {
                    order.Status = newStatus;
                    // Restore stock
                    foreach (var item in order.Items)
                    {
                        var product = _products.FirstOrDefault(p => p.Name == item.ProdName);
                        if (product != null)
                        {
                            product.StockQty += item.Qty;
                        }
                    }
                    return (true, "Order cancelled");
                }
                else
                {
                    return (false, "Cannot cancel shipped or delivered orders");
                }
            }
            else
            {
                return (false, "Invalid status");
            }
        }
        else
        {
            return (false, "Order not found");
        }
    }

    public (bool Success, string Message) ProcessPayment(int orderId, string paymentMethod)
    {
        var order = _orders.FirstOrDefault(o => o.Id == orderId);

        if (order != null)
        {
            if (!order.IsPaid)
            {
                if (paymentMethod == "CreditCard")
                {
                    // Simular processamento de cartão
                    order.IsPaid = true;
                    order.PaymentMethod = paymentMethod;
                    return (true, "Payment processed via Credit Card");
                }
                else if (paymentMethod == "DebitCard")
                {
                    order.IsPaid = true;
                    order.PaymentMethod = paymentMethod;
                    return (true, "Payment processed via Debit Card");
                }
                else if (paymentMethod == "Pix")
                {
                    order.IsPaid = true;
                    order.PaymentMethod = paymentMethod;
                    return (true, "Payment processed via Pix");
                }
                else if (paymentMethod == "BankTransfer")
                {
                    order.IsPaid = true;
                    order.PaymentMethod = paymentMethod;
                    return (true, "Payment processed via Bank Transfer");
                }
                else
                {
                    return (false, "Invalid payment method");
                }
            }
            else
            {
                return (false, "Order already paid");
            }
        }
        else
        {
            return (false, "Order not found");
        }
    }

    public Order? GetOrderById(int id)
    {
        return _orders.FirstOrDefault(o => o.Id == id);
    }

    public List<Order> GetAllOrders()
    {
        return _orders;
    }

    public List<Order> GetOrdersByStatus(string status)
    {
        return _orders.Where(o => o.Status == status).ToList();
    }

    public List<Product> GetAllProducts()
    {
        return _products;
    }

    // ❌ Cálculo de relatório com múltiplos níveis
    public object GenerateReport(DateTime? startDate, DateTime? endDate)
    {
        var filteredOrders = _orders.AsEnumerable();

        if (startDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.CreatedDt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.CreatedDt <= endDate.Value);
        }

        var orderList = filteredOrders.ToList();

        decimal totalRevenue = 0;
        decimal totalDiscount = 0;
        int totalItems = 0;
        int paidOrders = 0;
        int pendingOrders = 0;
        int cancelledOrders = 0;

        foreach (var order in orderList)
        {
            totalRevenue += order.TotalAmt;
            totalDiscount += order.DiscAmt;
            
            foreach (var item in order.Items)
            {
                totalItems += item.Qty;
            }

            if (order.Status == "Cancelled")
            {
                cancelledOrders++;
            }
            else if (order.IsPaid)
            {
                paidOrders++;
            }
            else
            {
                pendingOrders++;
            }
        }

        return new
        {
            TotalOrders = orderList.Count,
            TotalRevenue = totalRevenue,
            TotalDiscount = totalDiscount,
            TotalItems = totalItems,
            PaidOrders = paidOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            AverageOrderValue = orderList.Count > 0 ? totalRevenue / orderList.Count : 0
        };
    }
}
