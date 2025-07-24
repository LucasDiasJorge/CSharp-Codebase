using System;
using State.States;

namespace State;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==== Order State Pattern Demo ====");
        Console.WriteLine();
        
        // Create a new order
        Order order = new Order("12345", "John Doe", 99.99m);
        order.DisplayStatus();
        
        // Process through the different states
        Console.WriteLine("1. Attempting to ship before payment...");
        order.Ship();
        Console.WriteLine();
        
        Console.WriteLine("2. Processing payment...");
        order.ProcessPayment();
        order.DisplayStatus();
        
        Console.WriteLine("3. Attempting to deliver before shipping...");
        order.Deliver();
        Console.WriteLine();
        
        Console.WriteLine("4. Shipping the order...");
        order.Ship();
        order.DisplayStatus();
        
        Console.WriteLine("5. Attempting to cancel after shipping...");
        order.Cancel();
        Console.WriteLine();
        
        Console.WriteLine("6. Delivering the order...");
        order.Deliver();
        order.DisplayStatus();
        
        Console.WriteLine("7. Attempting to process payment again...");
        order.ProcessPayment();
        Console.WriteLine();
        
        // Create another order to demonstrate cancellation
        Console.WriteLine("==== Order Cancellation Demo ====");
        Console.WriteLine();
        
        Order cancelOrder = new Order("67890", "Jane Smith", 149.99m);
        cancelOrder.DisplayStatus();
        
        Console.WriteLine("1. Cancelling the new order...");
        cancelOrder.Cancel();
        cancelOrder.DisplayStatus();
        
        Console.WriteLine("2. Attempting to process payment for cancelled order...");
        cancelOrder.ProcessPayment();
        
        Console.WriteLine();
        Console.WriteLine("State Pattern allows objects to change behavior when their internal state changes.");
        Console.WriteLine("This implementation demonstrates how an Order's behavior changes as it moves through different states.");
    }
}
