# Unit of Work Pattern

## Descrição

O **Unit of Work** mantém uma lista de objetos afetados por uma transação de negócio e coordena a gravação das mudanças e resolução de problemas de concorrência.

## Estrutura

```
UnitOfWork/
├── Entities/
│   ├── Order.cs
│   ├── OrderStatus.cs
│   ├── Payment.cs
│   └── PaymentStatus.cs
├── Interfaces/
│   ├── IOrderRepository.cs
│   ├── IPaymentRepository.cs
│   └── IOrderUnitOfWork.cs
├── Implementations/
│   ├── InMemoryOrderRepository.cs
│   ├── InMemoryPaymentRepository.cs
│   └── OrderUnitOfWork.cs
└── README.md
```

## Diagrama

```
┌────────────────────────────────────────────────────────┐
│                    Unit of Work                         │
├────────────────────────────────────────────────────────┤
│  ┌──────────────┐  ┌──────────────┐                    │
│  │OrderRepository│  │PaymentRepository│                │
│  └──────────────┘  └──────────────┘                    │
│                                                         │
│  BeginTransaction() ─────────────────────▶ Backup      │
│  SaveChanges() ──────────────────────────▶ Persist     │
│  Commit() ───────────────────────────────▶ Confirm     │
│  Rollback() ─────────────────────────────▶ Restore     │
└────────────────────────────────────────────────────────┘
```

## Benefícios

✅ **Atomicidade** - Tudo ou nada  
✅ **Consistência** - Estado sempre válido  
✅ **Performance** - Batch de operações  
✅ **Simplificação** - Um ponto de controle  

## Exemplo de Uso

```csharp
public async Task ProcessOrderAsync(OrderRequest request)
{
    using var unitOfWork = new OrderUnitOfWork();
    
    await unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Criar pedido
        var order = Order.Create(request.CustomerId, request.Amount);
        await unitOfWork.Orders.AddAsync(order);
        
        // Criar pagamento
        var payment = Payment.Create(order.Id, request.Amount);
        await unitOfWork.Payments.AddAsync(payment);
        
        // Processar pagamento...
        payment.Approve();
        order.Confirm();
        
        await unitOfWork.SaveChangesAsync();
        await unitOfWork.CommitAsync();
    }
    catch
    {
        await unitOfWork.RollbackAsync();
        throw;
    }
}
```

## Combinação com Repository

O Unit of Work geralmente agrupa múltiplos repositórios:

```csharp
public interface IOrderUnitOfWork : IUnitOfWork
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }
    IInventoryRepository Inventory { get; }
}
```
