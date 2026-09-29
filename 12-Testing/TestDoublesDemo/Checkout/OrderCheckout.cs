using TestDoublesDemo.Abstractions;
using TestDoublesDemo.Domain;

namespace TestDoublesDemo.Checkout;

public interface IOrderCheckout
{
    CheckoutResult Place(Order order);
}

/// <summary>
/// A regra do checkout, igual nas duas implementações. O que varia entre elas é
/// apenas <see cref="Notify"/> — e isso é de propósito: é o que permite mostrar que
/// verificação de estado e verificação de comportamento não são equivalentes.
/// </summary>
public abstract class OrderCheckoutBase : IOrderCheckout
{
    private readonly IInventory _inventory;
    private readonly IOrderRepository _repository;
    private readonly IAuditLog _auditLog;

    protected OrderCheckoutBase(
        IInventory inventory,
        IOrderRepository repository,
        INotificationSender notifications,
        IAuditLog auditLog)
    {
        _inventory = inventory;
        _repository = repository;
        Notifications = notifications;
        _auditLog = auditLog;
    }

    protected INotificationSender Notifications { get; }

    public CheckoutResult Place(Order order)
    {
        // Este caminho nao toca em NENHUM colaborador: e o que torna o dummy possivel.
        if (order.Lines.Count == 0)
        {
            order.Reject();

            return CheckoutResult.Rejected(CheckoutOutcome.EmptyOrder, "pedido sem itens");
        }

        foreach (OrderLine line in order.Lines)
        {
            if (_inventory.HasStock(line.Sku, line.Quantity))
            {
                continue;
            }

            // Aqui o audit log e usado de verdade — entao aqui um dummy quebraria.
            _auditLog.Record($"pedido {order.Id} recusado: sem estoque de {line.Sku}");
            order.Reject();

            return CheckoutResult.Rejected(CheckoutOutcome.OutOfStock, $"sem estoque de {line.Sku}");
        }

        order.Confirm();
        _repository.Save(order);
        Notify(order);

        return CheckoutResult.Confirmed();
    }

    protected abstract void Notify(Order order);
}

/// <summary>
/// Notifica um item por vez. Foi a primeira implementação.
/// </summary>
public sealed class PerItemOrderCheckout : OrderCheckoutBase
{
    public PerItemOrderCheckout(
        IInventory inventory,
        IOrderRepository repository,
        INotificationSender notifications,
        IAuditLog auditLog)
        : base(inventory, repository, notifications, auditLog)
    {
    }

    protected override void Notify(Order order)
    {
        foreach (OrderLine line in order.Lines)
        {
            Notifications.Send(order.CustomerEmail, $"item confirmado: {line.Sku} x{line.Quantity}");
        }
    }
}

/// <summary>
/// Notifica uma vez, com todos os itens agrupados. É a refatoração que qualquer
/// revisor aprovaria: menos e-mail para o cliente, mesmo resultado de negócio.
///
/// É exatamente essa mudança que derruba um teste escrito sobre contagem de chamadas.
/// </summary>
public sealed class BatchedOrderCheckout : OrderCheckoutBase
{
    public BatchedOrderCheckout(
        IInventory inventory,
        IOrderRepository repository,
        INotificationSender notifications,
        IAuditLog auditLog)
        : base(inventory, repository, notifications, auditLog)
    {
    }

    protected override void Notify(Order order)
    {
        string items = string.Join(", ", order.Lines.Select(line => $"{line.Sku} x{line.Quantity}"));

        Notifications.Send(order.CustomerEmail, $"pedido confirmado: {items}");
    }
}
