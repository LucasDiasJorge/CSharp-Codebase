using System;
using State.States;

namespace State;

/// <summary>
/// Context class that maintains an instance of a concrete state subclass
/// that defines the current state of the order.
/// </summary>
public class Order
{
    // Current state of the order
    private IOrderState _currentState;
    
    // Available states
    public IOrderState NewState { get; private set; }
    public IOrderState PaidState { get; private set; }
    public IOrderState ShippedState { get; private set; }
    public IOrderState DeliveredState { get; private set; }
    public IOrderState CancelledState { get; private set; }
    
    // Order details
    public string OrderNumber { get; }
    public string CustomerName { get; }
    public decimal Total { get; }
    
    public Order(string orderNumber, string customerName, decimal total)
    {
        OrderNumber = orderNumber;
        CustomerName = customerName;
        Total = total;
        
        // Initialize states
        NewState = new NewOrderState(this);
        PaidState = new PaidOrderState(this);
        ShippedState = new ShippedOrderState(this);
        DeliveredState = new DeliveredOrderState(this);
        CancelledState = new CancelledOrderState(this);
        
        // Set initial state
        _currentState = NewState;
    }
    
    // State transition method
    public void TransitionToState(IOrderState state)
    {
        _currentState = state;
    }
    
    // Methods that delegate to the current state
    public void ProcessPayment()
    {
        _currentState.ProcessPayment();
    }
    
    public void Ship()
    {
        _currentState.Ship();
    }
    
    public void Deliver()
    {
        _currentState.Deliver();
    }
    
    public void Cancel()
    {
        _currentState.Cancel();
    }
    
    // Method to display current state
    public void DisplayStatus()
    {
        Console.WriteLine($"Order #{OrderNumber} for {CustomerName}");
        Console.WriteLine($"Total: ${Total}");
        Console.WriteLine($"Current State: {_currentState.GetStateName()}");
        Console.WriteLine("----------------------------------------");
    }
}
