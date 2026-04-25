using System;

namespace State.States;

/// <summary>
/// Concrete state implementation for a cancelled order state.
/// </summary>
public class CancelledOrderState : IOrderState
{
    private readonly Order _order;
    
    public CancelledOrderState(Order order)
    {
        _order = order;
    }
    
    public void ProcessPayment()
    {
        Console.WriteLine("Cannot process payment for order #" + _order.OrderNumber + " - Order is cancelled.");
    }
    
    public void Ship()
    {
        Console.WriteLine("Cannot ship order #" + _order.OrderNumber + " - Order is cancelled.");
    }
    
    public void Deliver()
    {
        Console.WriteLine("Cannot deliver order #" + _order.OrderNumber + " - Order is cancelled.");
    }
    
    public void Cancel()
    {
        Console.WriteLine("Order #" + _order.OrderNumber + " is already cancelled.");
    }
    
    public string GetStateName()
    {
        return "Cancelled";
    }
}
