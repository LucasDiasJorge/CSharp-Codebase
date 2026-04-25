# Unit of Work Pattern

## Visão geral

Projeto didático do CSharp-101 dedicado a Unit of Work Pattern, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Unit of Work Pattern no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Unit of Work Pattern se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
UnitOfWork/
+-- Entities/
|   +-- Order.cs
|   +-- OrderStatus.cs
|   +-- Payment.cs
|   \-- PaymentStatus.cs
+-- Implementations/
|   +-- InMemoryOrderRepository.cs
|   +-- InMemoryPaymentRepository.cs
|   \-- OrderUnitOfWork.cs
\-- Interfaces/
    +-- IOrderRepository.cs
    +-- IOrderUnitOfWork.cs
    \-- IPaymentRepository.cs
```

## Como executar

Consulte o código desta pasta e os projetos relacionados antes de executar comandos específicos.

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

O **Unit of Work** mantém uma lista de objetos afetados por uma transação de negócio e coordena a gravação das mudanças e resolução de problemas de concorrência.

##### Estrutura

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

##### Diagrama

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

##### Benefícios

✅ **Atomicidade** - Tudo ou nada  
✅ **Consistência** - Estado sempre válido  
✅ **Performance** - Batch de operações  
✅ **Simplificação** - Um ponto de controle

##### Exemplo de Uso

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

##### Combinação com Repository

O Unit of Work geralmente agrupa múltiplos repositórios:

```csharp
public interface IOrderUnitOfWork : IUnitOfWork
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }
    IInventoryRepository Inventory { get; }
}
```
