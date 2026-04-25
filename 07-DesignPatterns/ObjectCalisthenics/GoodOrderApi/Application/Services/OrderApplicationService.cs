using GoodOrderApi.Domain.Entities;
using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Application.Services;

// ✅ REGRA 1: Apenas um nível de indentação por método
// ✅ REGRA 2: Não usar ELSE - usar early returns
// ✅ REGRA 5: Um ponto por linha
// ✅ REGRA 7: Manter entidades pequenas - Service focado

/// <summary>
/// Serviço de aplicação para gerenciamento de pedidos.
/// Orquestra as operações do domínio.
/// </summary>
public class OrderApplicationService
{
    private readonly OrderRepository _orderRepository;
    private readonly ProductRepository _productRepository;

    public OrderApplicationService(OrderRepository orderRepository, ProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    // ✅ REGRA 1: Um nível de indentação
    public Result<Order> CreateOrder(CreateOrderCommand command)
    {
        var validationResult = ValidateCommand(command);
        if (validationResult.IsFailure)
            return Result<Order>.Failure(validationResult.Error);

        var customerInfo = CreateCustomerInfo(command);
        var order = _orderRepository.CreateOrder(customerInfo);
        
        var itemsResult = AddItemsToOrder(order, command.Items);
        if (itemsResult.IsFailure)
            return Result<Order>.Failure(itemsResult.Error);

        ApplyDiscountIfPresent(order, command.DiscountCode);
        
        return Result<Order>.Success(order);
    }

    // ✅ REGRA 1: Métodos extraídos para manter um nível de indentação
    private Result ValidateCommand(CreateOrderCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.CustomerName))
            return Result.Failure("Customer name is required");

        if (string.IsNullOrWhiteSpace(command.CustomerEmail))
            return Result.Failure("Customer email is required");

        if (command.Items.Count == 0)
            return Result.Failure("Order must have at least one item");

        return Result.Success();
    }

    private CustomerInfo CreateCustomerInfo(CreateOrderCommand command)
    {
        return CustomerInfo.Create(
            command.CustomerName,
            command.CustomerEmail,
            command.CustomerPhone,
            command.CustomerAddress,
            command.CustomerCity,
            command.CustomerZip
        );
    }

    // ✅ REGRA 1: Um nível de indentação - lógica de loop extraída
    private Result AddItemsToOrder(Order order, IReadOnlyList<OrderItemCommand> items)
    {
        foreach (var itemCommand in items)
        {
            var result = AddSingleItem(order, itemCommand);
            if (result.IsFailure)
                return result;
        }
        return Result.Success();
    }

    private Result AddSingleItem(Order order, OrderItemCommand itemCommand)
    {
        var product = _productRepository.FindById(itemCommand.ProductId);
        if (product is null)
            return Result.Failure($"Product with ID {itemCommand.ProductId} not found");

        if (!product.IsAvailable)
            return Result.Failure($"Product {product.Info.Identification.Name} is not available");

        var quantity = Quantity.Create(itemCommand.Quantity);
        
        if (!product.HasSufficientStock(quantity))
            return Result.Failure($"Insufficient stock for {product.Info.Identification.Name}");

        var orderItem = CreateOrderItem(order, product, quantity);
        order.AddItem(orderItem);
        product.ReserveStock(quantity);

        return Result.Success();
    }

    private OrderItem CreateOrderItem(Order order, Product product, Quantity quantity)
    {
        var itemId = order.Financials.Items.Count + 1;
        return OrderItem.Create(itemId, product, quantity);
    }

    private void ApplyDiscountIfPresent(Order order, string? discountCode)
    {
        var discount = DiscountCode.TryCreate(discountCode);
        order.ApplyDiscount(discount);
    }

    // ✅ REGRA 2: Sem ELSE - using pattern matching
    public Result UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = _orderRepository.FindById(orderId);
        if (order is null)
            return Result.Failure("Order not found");

        try
        {
            ExecuteStatusTransition(order, newStatus);
            return Result.Success($"Order status updated to {newStatus}");
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    // ✅ REGRA 2: Sem ELSE - usando switch expression
    private void ExecuteStatusTransition(Order order, string newStatus)
    {
        Action transitionAction = newStatus switch
        {
            "Confirmed" => order.Confirm,
            "Shipped" => order.Ship,
            "Delivered" => order.Deliver,
            "Cancelled" => () => CancelOrder(order),
            _ => throw new DomainException($"Invalid status: {newStatus}")
        };

        transitionAction();
    }

    private void CancelOrder(Order order)
    {
        order.Cancel(item => RestoreProductStock(item));
    }

    private void RestoreProductStock(OrderItem item)
    {
        var productName = item.Details.Product.Name;
        var product = _productRepository.FindByName(productName);
        product?.RestoreStock(item.GetQuantity());
    }

    public Result ProcessPayment(int orderId, string paymentMethodName)
    {
        var order = _orderRepository.FindById(orderId);
        if (order is null)
            return Result.Failure("Order not found");

        if (order.State.IsPaid)
            return Result.Failure("Order already paid");

        try
        {
            var paymentMethod = PaymentMethod.Create(paymentMethodName);
            order.MarkAsPaid(paymentMethod);
            return Result.Success($"Payment processed via {paymentMethod.DisplayName}");
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    public Order? GetOrderById(int id) => _orderRepository.FindById(id);
    
    public IReadOnlyList<Order> GetAllOrders() => _orderRepository.GetAll();
    
    public IReadOnlyList<Order> GetOrdersByStatus(string status) => _orderRepository.FindByStatus(status);
    
    public IReadOnlyList<Product> GetAllProducts() => _productRepository.GetAll();
}

// ✅ REGRA 7: Classes pequenas e focadas
public sealed class CreateOrderCommand
{
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerEmail { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public string CustomerAddress { get; init; } = string.Empty;
    public string CustomerCity { get; init; } = string.Empty;
    public string CustomerZip { get; init; } = string.Empty;
    public IReadOnlyList<OrderItemCommand> Items { get; init; } = Array.Empty<OrderItemCommand>();
    public string? DiscountCode { get; init; }
}

public sealed class OrderItemCommand
{
    public int ProductId { get; init; }
    public int Quantity { get; init; }
}

// ✅ Result pattern - evita exceções para fluxo de controle
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string Message { get; }

    protected Result(bool isSuccess, string error, string message)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public static Result Success(string message = "") => new(true, string.Empty, message);
    public static Result Failure(string error) => new(false, error, string.Empty);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, string error, T? value) 
        : base(isSuccess, error, string.Empty)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, string.Empty, value);
    public new static Result<T> Failure(string error) => new(false, error, default);
}
