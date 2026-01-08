using GoodOrderApi.Domain.ValueObjects;

namespace GoodOrderApi.Domain.Entities;

// ✅ REGRA 1: Apenas um nível de indentação por método
// ✅ REGRA 2: Não usar ELSE - usar early returns
// ✅ REGRA 4: First Class Collections - OrderItems
// ✅ REGRA 8: Máximo duas variáveis de instância (agrupamos)
// ✅ REGRA 9: Expor comportamento em vez de getters/setters

/// <summary>
/// Agregado raiz que representa um pedido.
/// </summary>
public class Order
{
    public int Id { get; }
    public OrderCustomerData CustomerData { get; }
    public OrderFinancials Financials { get; private set; }
    public OrderState State { get; private set; }

    private Order(int id, OrderCustomerData customerData, OrderFinancials financials, OrderState state)
    {
        Id = id;
        CustomerData = customerData;
        Financials = financials;
        State = state;
    }

    public static Order Create(int id, CustomerInfo customerInfo)
    {
        var customerData = OrderCustomerData.Create(customerInfo);
        var financials = OrderFinancials.Initial();
        var state = OrderState.Initial();
        
        return new Order(id, customerData, financials, state);
    }

    // ✅ REGRA 1: Um nível de indentação
    // ✅ REGRA 9: Comportamento em vez de setter
    public void AddItem(OrderItem item)
    {
        State.EnsureCanModify();
        Financials = Financials.AddItem(item);
    }

    public void ApplyDiscount(DiscountCode? discountCode)
    {
        if (discountCode is null) return;
        
        Financials = Financials.ApplyDiscount(discountCode);
    }

    // ✅ REGRA 2: Sem ELSE - early return
    public void Confirm()
    {
        var newStatus = OrderStatus.FromString("Confirmed");
        EnsureValidTransition(newStatus);
        State = State.TransitionTo(newStatus);
    }

    public void Ship()
    {
        EnsurePaymentForShipping();
        
        var newStatus = OrderStatus.FromString("Shipped");
        EnsureValidTransition(newStatus);
        State = State.Ship(newStatus);
    }

    public void Deliver()
    {
        var newStatus = OrderStatus.FromString("Delivered");
        EnsureValidTransition(newStatus);
        State = State.TransitionTo(newStatus);
    }

    public void Cancel(Action<OrderItem> restoreStockAction)
    {
        EnsureCanCancel();
        
        var newStatus = OrderStatus.FromString("Cancelled");
        Financials.Items.ForEach(restoreStockAction);
        State = State.TransitionTo(newStatus);
    }

    public void MarkAsPaid(PaymentMethod paymentMethod)
    {
        State = State.MarkAsPaid(paymentMethod);
    }

    // ✅ REGRA 1: Métodos pequenos com um nível de indentação
    private void EnsureValidTransition(OrderStatus newStatus)
    {
        if (!State.Status.CanTransitionTo(newStatus))
            throw new DomainException($"Cannot transition from {State.Status.Name} to {newStatus.Name}");
    }

    private void EnsurePaymentForShipping()
    {
        if (State.Status.RequiresPaymentForShipping && !State.IsPaid)
            throw new DomainException("Order must be paid before shipping");
    }

    private void EnsureCanCancel()
    {
        if (!State.Status.CanBeCancelled)
            throw new DomainException($"Cannot cancel order in {State.Status.Name} status");
    }
}

/// <summary>
/// Value Object com dados do cliente no pedido.
/// ✅ REGRA 8: Duas variáveis de instância.
/// </summary>
public sealed class OrderCustomerData
{
    public CustomerInfo Customer { get; }
    public DateTime CreatedAt { get; }

    private OrderCustomerData(CustomerInfo customer, DateTime createdAt)
    {
        Customer = customer;
        CreatedAt = createdAt;
    }

    public static OrderCustomerData Create(CustomerInfo customer)
    {
        return new OrderCustomerData(customer, DateTime.UtcNow);
    }
}

/// <summary>
/// Value Object com informações financeiras do pedido.
/// ✅ REGRA 8: Duas variáveis de instância.
/// </summary>
public sealed class OrderFinancials
{
    public OrderItems Items { get; }
    public DiscountInfo Discount { get; }

    private OrderFinancials(OrderItems items, DiscountInfo discount)
    {
        Items = items;
        Discount = discount;
    }

    public static OrderFinancials Initial()
    {
        return new OrderFinancials(OrderItems.Empty(), DiscountInfo.None());
    }

    public OrderFinancials AddItem(OrderItem item)
    {
        var newItems = Items.Add(item);
        return new OrderFinancials(newItems, Discount);
    }

    public OrderFinancials ApplyDiscount(DiscountCode discountCode)
    {
        var subtotal = Items.CalculateTotal();
        var newDiscount = DiscountInfo.From(discountCode, subtotal);
        return new OrderFinancials(Items, newDiscount);
    }

    public Money CalculateTotal()
    {
        var subtotal = Items.CalculateTotal();
        return subtotal.Subtract(Discount.Amount);
    }
}

/// <summary>
/// Value Object com informações de desconto.
/// ✅ REGRA 8: Duas variáveis de instância.
/// </summary>
public sealed class DiscountInfo
{
    public string? Code { get; }
    public Money Amount { get; }

    private DiscountInfo(string? code, Money amount)
    {
        Code = code;
        Amount = amount;
    }

    public static DiscountInfo None() => new(null, Money.Zero);

    public static DiscountInfo From(DiscountCode discountCode, Money subtotal)
    {
        var discountAmount = discountCode.CalculateDiscountAmount(subtotal);
        return new DiscountInfo(discountCode.Code, discountAmount);
    }
}

/// <summary>
/// Value Object com estado do pedido.
/// ✅ REGRA 8: Duas variáveis de instância (agrupadas).
/// </summary>
public sealed class OrderState
{
    public OrderStatus Status { get; }
    public PaymentInfo Payment { get; }

    private OrderState(OrderStatus status, PaymentInfo payment)
    {
        Status = status;
        Payment = payment;
    }

    public static OrderState Initial()
    {
        return new OrderState(new PendingStatus(), PaymentInfo.NotPaid());
    }

    public bool IsPaid => Payment.IsPaid;

    public void EnsureCanModify()
    {
        if (Status is not PendingStatus)
            throw new DomainException("Cannot modify order that is not pending");
    }

    public OrderState TransitionTo(OrderStatus newStatus)
    {
        return new OrderState(newStatus, Payment);
    }

    public OrderState Ship(OrderStatus newStatus)
    {
        var shippedPayment = Payment.WithShippedDate(DateTime.UtcNow);
        return new OrderState(newStatus, shippedPayment);
    }

    public OrderState MarkAsPaid(PaymentMethod method)
    {
        var paidPayment = Payment.MarkAsPaid(method);
        return new OrderState(Status, paidPayment);
    }
}

/// <summary>
/// Value Object com informações de pagamento.
/// ✅ REGRA 8: Duas variáveis de instância.
/// </summary>
public sealed class PaymentInfo
{
    public PaymentStatus Status { get; }
    public DateTime? ShippedAt { get; }

    private PaymentInfo(PaymentStatus status, DateTime? shippedAt)
    {
        Status = status;
        ShippedAt = shippedAt;
    }

    public static PaymentInfo NotPaid() => new(PaymentStatus.NotPaid(), null);

    public bool IsPaid => Status.IsPaid;

    public PaymentInfo MarkAsPaid(PaymentMethod method)
    {
        return new PaymentInfo(PaymentStatus.Paid(method), ShippedAt);
    }

    public PaymentInfo WithShippedDate(DateTime date)
    {
        return new PaymentInfo(Status, date);
    }
}

/// <summary>
/// Value Object representando o status do pagamento.
/// </summary>
public sealed class PaymentStatus
{
    public bool IsPaid { get; }
    public PaymentMethod? Method { get; }

    private PaymentStatus(bool isPaid, PaymentMethod? method)
    {
        IsPaid = isPaid;
        Method = method;
    }

    public static PaymentStatus NotPaid() => new(false, null);
    public static PaymentStatus Paid(PaymentMethod method) => new(true, method);
}
