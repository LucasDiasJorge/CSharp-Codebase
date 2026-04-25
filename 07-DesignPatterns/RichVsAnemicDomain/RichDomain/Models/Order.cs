namespace RichDomain.Models;

/// <summary>
/// Exemplo de Modelo Rico: Dados + Comportamento + Validação
/// ✅ Esta classe protege suas invariantes e encapsula lógica de negócio
/// </summary>
public class Order
{
    // ✅ Propriedades com setters privados - proteção
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // ✅ Coleções imutáveis publicamente
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    // ✅ Total calculado automaticamente
    public decimal Total => CalculateTotal();
    
    private decimal _discountPercentage;
    public decimal DiscountPercentage => _discountPercentage;
    
    // ✅ Construtor privado - força uso de factory method
    private Order(string customerName)
    {
        Id = Guid.NewGuid();
        CustomerName = customerName;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        _discountPercentage = 0;
    }
    
    /// <summary>
    /// Factory Method: maneira correta de criar um pedido
    /// ✅ Validação no momento da criação
    /// </summary>
    public static Order Create(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Nome do cliente é obrigatório", nameof(customerName));
        
        return new Order(customerName);
    }
    
    /// <summary>
    /// Adiciona item ao pedido
    /// ✅ Comportamento encapsulado no domínio
    /// ✅ Total recalculado automaticamente
    /// </summary>
    public void AddItem(string productName, int quantity, decimal unitPrice)
    {
        // ✅ Regra de negócio: só pode adicionar se estiver pendente
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode adicionar itens em pedido pendente");
        
        // ✅ Criação com validação
        var item = OrderItem.Create(productName, quantity, unitPrice);
        
        _items.Add(item);
        
        // ✅ Não precisa chamar RecalculateTotal() - é automático!
    }
    
    /// <summary>
    /// Remove item do pedido
    /// ✅ Validação e comportamento no domínio
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode remover itens em pedido pendente");
        
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        _items.Remove(item);
    }
    
    /// <summary>
    /// Aplica desconto ao pedido
    /// ✅ Regras de negócio protegidas
    /// </summary>
    public void ApplyDiscount(decimal discountPercentage)
    {
        // ✅ Validação de invariante
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Desconto deve estar entre 0 e 100", nameof(discountPercentage));
        
        // ✅ Regra de negócio
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode aplicar desconto em pedido pendente");
        
        _discountPercentage = discountPercentage;
    }
    
    /// <summary>
    /// Processa o pedido
    /// ✅ Transição de estado controlada
    /// </summary>
    public void Process()
    {
        // ✅ Valida estado atual
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode processar pedido pendente");
        
        // ✅ Valida regra de negócio
        if (!_items.Any())
            throw new InvalidOperationException("Pedido sem itens não pode ser processado");
        
        Status = OrderStatus.Processing;
    }
    
    /// <summary>
    /// Marca pedido como enviado
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException("Só pode enviar pedido em processamento");
        
        Status = OrderStatus.Shipped;
    }
    
    /// <summary>
    /// Marca pedido como entregue
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Só pode entregar pedido enviado");
        
        Status = OrderStatus.Delivered;
    }
    
    /// <summary>
    /// Cancela o pedido
    /// ✅ Regra de negócio: só pode cancelar se pendente
    /// </summary>
    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode cancelar pedido pendente");
        
        Status = OrderStatus.Cancelled;
    }
    
    /// <summary>
    /// Calcula o total do pedido
    /// ✅ Cálculo encapsulado e sempre consistente
    /// </summary>
    private decimal CalculateTotal()
    {
        var subtotal = _items.Sum(i => i.Subtotal);
        var discountAmount = subtotal * (_discountPercentage / 100);
        return subtotal - discountAmount;
    }
    
    /// <summary>
    /// Verifica se o pedido pode ser modificado
    /// ✅ Lógica de negócio no domínio
    /// </summary>
    public bool CanModify() => Status == OrderStatus.Pending;
    
    /// <summary>
    /// Verifica se o pedido pode ser cancelado
    /// </summary>
    public bool CanCancel() => Status == OrderStatus.Pending;
}

/// <summary>
/// Modelo Rico: Item de Pedido
/// ✅ Protege suas invariantes
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    // ✅ Subtotal calculado automaticamente - sempre consistente
    public decimal Subtotal => Quantity * UnitPrice;
    
    // ✅ Construtor privado
    private OrderItem(string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
    
    /// <summary>
    /// Factory Method com validação
    /// ✅ Garante que objeto sempre está válido
    /// </summary>
    public static OrderItem Create(string productName, int quantity, decimal unitPrice)
    {
        // ✅ Validações garantem invariantes
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Nome do produto é obrigatório", nameof(productName));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));
        
        if (unitPrice < 0)
            throw new ArgumentException("Preço não pode ser negativo", nameof(unitPrice));
        
        return new OrderItem(productName, quantity, unitPrice);
    }
    
    /// <summary>
    /// Atualiza quantidade
    /// ✅ Validação sempre aplicada
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(newQuantity));
        
        Quantity = newQuantity;
        // Subtotal é recalculado automaticamente!
    }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// ✅ Vantagens do Domínio Rico:
/// 
/// 1. ENCAPSULAMENTO FORTE
///    - Setters privados protegem contra modificações inválidas
///    - Só pode modificar através de métodos que validam
/// 
/// 2. INVARIANTES GARANTIDAS
///    - Impossível criar objeto em estado inválido
///    - Validações sempre aplicadas
/// 
/// 3. LÓGICA NO LUGAR CERTO
///    - Regras de negócio estão próximas aos dados
///    - Fácil encontrar e entender as regras
/// 
/// 4. AUTO-VALIDAÇÃO
///    - Objeto se valida
///    - Não precisa de validadores externos
/// 
/// 5. CÁLCULOS AUTOMÁTICOS
///    - Total e Subtotal sempre corretos
///    - Impossível ficar dessincronizado
/// 
/// 6. TESTABILIDADE
///    - Pode testar regras de negócio diretamente
///    - Não precisa de mocks
/// 
/// 7. EXPRESSIVIDADE
///    - Código reflete o domínio do negócio
///    - Fácil de entender e manter
/// 
/// 8. IMUTABILIDADE CONTROLADA
///    - Coleções read-only publicamente
///    - Mudanças só por métodos controlados
/// </summary>
