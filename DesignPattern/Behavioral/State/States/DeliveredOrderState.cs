using System;

namespace State.States;

/// <summary>
/// Concrete state implementation for a delivered order state.
/// </summary>
public class DeliveredOrderState : IOrderState
{
    private readonly Order _order;
    
    public DeliveredOrderState(Order order)
    {
        _order = order;
    }
    
    public void ProcessPayment()
    {
        Console.WriteLine("Payment for order #" + _order.OrderNumber + " has already been processed.");
    }
    
    public void Ship()
    {
        Console.WriteLine("Order #" + _order.OrderNumber + " has already been shipped and delivered.");
    }
    
    public void Deliver()
    {
        Console.WriteLine("Order #" + _order.OrderNumber + " has already been delivered.");
    }
    
    public void Cancel()
    {
        Console.WriteLine("Cannot cancel order #" + _order.OrderNumber + " - Already delivered. Please initiate a return.");
    }
    
    public string GetStateName()
    {
        return "Delivered";
    }
}
