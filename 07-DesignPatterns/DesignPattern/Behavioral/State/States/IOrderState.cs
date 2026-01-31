namespace State.States;

/// <summary>
/// Interface defining the contract for concrete order state classes.
/// Each state implements the behavior associated with a particular state of an order.
/// </summary>
public interface IOrderState
{
    /// <summary>
    /// Processes payment for the order in current state
    /// </summary>
    void ProcessPayment();
    
    /// <summary>
    /// Ships the order in current state
    /// </summary>
    void Ship();
    
    /// <summary>
    /// Delivers the order in current state
    /// </summary>
    void Deliver();
    
    /// <summary>
    /// Cancels the order in current state
    /// </summary>
    void Cancel();
    
    /// <summary>
    /// Gets the name of the current state
    /// </summary>
    string GetStateName();
}
