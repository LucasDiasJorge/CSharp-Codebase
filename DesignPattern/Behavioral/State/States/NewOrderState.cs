using System;

namespace State.States;

/// <summary>
/// Concrete state implementation for a new order state.
/// </summary>
public class NewOrderState : IOrderState
{
    private readonly Order _order;
    
    public NewOrderState(Order order)
    {
        _order = order;
    }
    
    public void ProcessPayment()
    {
        Console.WriteLine("Processing payment for order #" + _order.OrderNumber);
        Console.WriteLine("Payment processed successfully!");
        _order.TransitionToState(_order.PaidState);
    }
    
    public void Ship()
    {
        Console.WriteLine("Cannot ship order #" + _order.OrderNumber + " - Payment required first!");
    }
    
    public void Deliver()
    {
        Console.WriteLine("Cannot deliver order #" + _order.OrderNumber + " - Payment and shipping required first!");
    }
    
    public void Cancel()
    {
        Console.WriteLine("Order #" + _order.OrderNumber + " has been cancelled.");
        _order.TransitionToState(_order.CancelledState);
    }
    
    public string GetStateName()
    {
        return "New";
    }
}
