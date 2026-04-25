using System;

namespace State.States;

/// <summary>
/// Concrete state implementation for a shipped order state.
/// </summary>
public class ShippedOrderState : IOrderState
{
    private readonly Order _order;
    
    public ShippedOrderState(Order order)
    {
        _order = order;
    }
    
    public void ProcessPayment()
    {
        Console.WriteLine("Payment for order #" + _order.OrderNumber + " has already been processed.");
    }
    
    public void Ship()
    {
        Console.WriteLine("Order #" + _order.OrderNumber + " has already been shipped.");
    }
    
    public void Deliver()
    {
        Console.WriteLine("Delivering order #" + _order.OrderNumber);
        Console.WriteLine("Order has been delivered successfully!");
        _order.TransitionToState(_order.DeliveredState);
    }
    
    public void Cancel()
    {
        Console.WriteLine("Cannot cancel order #" + _order.OrderNumber + " - Already shipped. Please return after delivery.");
    }
    
    public string GetStateName()
    {
        return "Shipped";
    }
}
