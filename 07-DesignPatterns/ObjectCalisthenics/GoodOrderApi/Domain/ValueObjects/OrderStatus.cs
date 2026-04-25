namespace GoodOrderApi.Domain.ValueObjects;

// ✅ REGRA 3: Encapsular primitivos - Status como Value Object
// ✅ REGRA 2: Evitar ELSE - usando pattern matching e early returns
// ✅ REGRA 7: Manter entidades pequenas - cada status é responsável por si

/// <summary>
/// Value Object que representa o status de um pedido.
/// Encapsula as transições válidas de estado.
/// </summary>
public abstract class OrderStatus
{
    public abstract string Name { get; }
    
    public abstract bool CanTransitionTo(OrderStatus newStatus);
    
    public abstract bool CanBeCancelled { get; }
    
    public abstract bool RequiresPaymentForShipping { get; }

    public override string ToString() => Name;
    
    public static OrderStatus FromString(string status)
    {
        return status switch
        {
            "Pending" => new PendingStatus(),
            "Confirmed" => new ConfirmedStatus(),
            "Shipped" => new ShippedStatus(),
            "Delivered" => new DeliveredStatus(),
            "Cancelled" => new CancelledStatus(),
            _ => throw new DomainException($"Invalid order status: {status}")
        };
    }
}

public sealed class PendingStatus : OrderStatus
{
    public override string Name => "Pending";
    public override bool CanBeCancelled => true;
    public override bool RequiresPaymentForShipping => true;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return newStatus is ConfirmedStatus or CancelledStatus;
    }
}

public sealed class ConfirmedStatus : OrderStatus
{
    public override string Name => "Confirmed";
    public override bool CanBeCancelled => true;
    public override bool RequiresPaymentForShipping => true;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return newStatus is ShippedStatus or CancelledStatus;
    }
}

public sealed class ShippedStatus : OrderStatus
{
    public override string Name => "Shipped";
    public override bool CanBeCancelled => false;
    public override bool RequiresPaymentForShipping => false;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return newStatus is DeliveredStatus;
    }
}

public sealed class DeliveredStatus : OrderStatus
{
    public override string Name => "Delivered";
    public override bool CanBeCancelled => false;
    public override bool RequiresPaymentForShipping => false;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return false; // Estado final
    }
}

public sealed class CancelledStatus : OrderStatus
{
    public override string Name => "Cancelled";
    public override bool CanBeCancelled => false;
    public override bool RequiresPaymentForShipping => false;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return false; // Estado final
    }
}
