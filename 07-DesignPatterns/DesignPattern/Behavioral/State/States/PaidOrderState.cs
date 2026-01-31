using System;

namespace State.States;

/// <summary>
/// Concrete state implementation for a paid order state.
/// </summary>
public class PaidOrderState : IOrderState
{
    private readonly Order _order;
    
    public PaidOrderState(Order order)
    {
        _order = order;
    }
    
    public void ProcessPayment()
    {
        Console.WriteLine("Payment for order #" + _order.OrderNumber + " has already been processed.");
    }
    
    public void Ship()
    {
        Console.WriteLine("Shipping order #" + _order.OrderNumber);
        Console.WriteLine("Order has been shipped!");
        _order.TransitionToState(_order.ShippedState);
    }
    
    public void Deliver()
    {
        Console.WriteLine("Cannot deliver order #" + _order.OrderNumber + " - Shipping required first!");
    }
    
    public void Cancel()
    {
        Console.WriteLine("Order #" + _order.OrderNumber + " has been cancelled. Refund will be processed.");
        _order.TransitionToState(_order.CancelledState);
    }
    
    public string GetStateName()
    {
        return "Paid";
    }
}
