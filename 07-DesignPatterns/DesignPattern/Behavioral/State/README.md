# State Design Pattern

## Overview
The State pattern is a behavioral design pattern that allows an object to alter its behavior when its internal state changes. The object will appear to change its class.

## Implementation
This implementation demonstrates the State pattern using an Order processing system:

1. **Context (Order)**: Maintains a reference to a concrete state object and delegates state-specific behavior to it.
2. **State Interface (IOrderState)**: Defines an interface for encapsulating behavior associated with a particular state of the Order.
3. **Concrete States**:
   - **NewOrderState**: Initial state of an order when it's created but not yet paid.
   - **PaidOrderState**: State after payment has been processed successfully.
   - **ShippedOrderState**: State after the order has been shipped.
   - **DeliveredOrderState**: State after the order has been delivered to the customer.
   - **CancelledOrderState**: State when an order has been cancelled.

## When to Use
- When an object's behavior depends on its state and must change at runtime according to that state
- When operations have large, multipart conditional statements that depend on the object's state
- When state transitions are explicit and complex

## Benefits
- Eliminates conditional statements by encapsulating state-specific behavior in separate classes
- Simplifies the code in the context (Order class)
- Makes state transitions explicit
- Allows new states to be added without changing existing state classes or the context

## Structure
```
Order (Context)
 ↑
 |
 +--> IOrderState (State Interface)
       ↑
       |
       +---> NewOrderState
       |
       +---> PaidOrderState
       |
       +---> ShippedOrderState
       |
       +---> DeliveredOrderState
       |
       +---> CancelledOrderState
```

## Example Usage
The demo in `Program.cs` shows how an Order object transitions through different states and how its behavior changes accordingly.
