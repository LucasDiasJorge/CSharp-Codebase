namespace CompositionOrderFulfillment.Models;

public enum PurchaseOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Cancelled = 3
}

public sealed class PurchaseOrder
{
    private readonly List<OrderLine> orderLines;

    public PurchaseOrder(string customerName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);

        Id = Guid.NewGuid();
        CustomerName = customerName;
        CreatedAt = DateTime.UtcNow;
        Status = PurchaseOrderStatus.Draft;
        orderLines = new List<OrderLine>();
    }

    public Guid Id { get; }
    public string CustomerName { get; }
    public DateTime CreatedAt { get; }
    public PurchaseOrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderLine> OrderLines => orderLines.AsReadOnly();

    public OrderLine AddItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException("Itens so podem ser adicionados em pedidos no status Draft.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        OrderLine orderLine = new OrderLine(
            product.Sku,
            product.Description,
            product.UnitPrice,
            quantity,
            this);

        orderLines.Add(orderLine);
        return orderLine;
    }

    public void Confirm()
    {
        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Pedido cancelado nao pode ser confirmado.");
        }

        if (orderLines.Count == 0)
        {
            throw new InvalidOperationException("Pedido sem itens nao pode ser confirmado.");
        }

        Status = PurchaseOrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Cancelled)
        {
            return;
        }

        foreach (OrderLine orderLine in orderLines)
        {
            orderLine.Deactivate();
        }

        orderLines.Clear();
        Status = PurchaseOrderStatus.Cancelled;
    }

    public decimal GetTotalAmount()
    {
        decimal totalAmount = 0m;

        foreach (OrderLine orderLine in orderLines)
        {
            if (orderLine.IsActive)
            {
                totalAmount += orderLine.Subtotal;
            }
        }

        return totalAmount;
    }

    public sealed class OrderLine
    {
        private readonly PurchaseOrder owner;
        private bool isActive;

        internal OrderLine(
            string productSku,
            string description,
            decimal unitPrice,
            int quantity,
            PurchaseOrder owner)
        {
            ProductSku = productSku;
            Description = description;
            UnitPrice = unitPrice;
            Quantity = quantity;
            this.owner = owner;
            isActive = true;
        }

        public string ProductSku { get; }
        public string Description { get; }
        public decimal UnitPrice { get; }
        public int Quantity { get; }
        public bool IsActive => isActive;
        public PurchaseOrder Owner => owner;
        public decimal Subtotal => UnitPrice * Quantity;

        internal void Deactivate()
        {
            isActive = false;
        }

        public override string ToString()
        {
            string status = IsActive ? "ATIVO" : "INATIVO";
            return $"{ProductSku} | {Description} | Qtd: {Quantity} | Subtotal: {Subtotal:C} | {status}";
        }
    }
}