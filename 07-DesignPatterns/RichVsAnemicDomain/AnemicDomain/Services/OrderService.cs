using AnemicDomain.Models;

namespace AnemicDomain.Services;

/// <summary>
/// Problema: Toda a lógica de negócio está aqui, não no domínio
/// Este serviço conhece TODOS os detalhes internos do pedido
/// </summary>
public class OrderService
{
    /// <summary>
    /// Cria um novo pedido
    /// ❌ Problema: Validação espalhada, difícil de manter
    /// </summary>
    public Order CreateOrder(string customerName)
    {
        // ❌ Validação manual em serviço
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Nome do cliente é obrigatório");
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            Items = new List<OrderItem>(),
            Total = 0,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Discount = 0
        };
        
        return order;
    }
    
    /// <summary>
    /// Adiciona item ao pedido
    /// ❌ Problema: Lógica de cálculo está no serviço, não no domínio
    /// </summary>
    public void AddItem(Order order, string productName, int quantity, decimal unitPrice)
    {
        // ❌ Validações manuais espalhadas
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Nome do produto é obrigatório");
        
        if (quantity <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");
        
        if (unitPrice < 0)
            throw new ArgumentException("Preço não pode ser negativo");
        
        // ❌ Cálculo manual - pode ser esquecido ou feito errado
        var subtotal = quantity * unitPrice;
        
        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = subtotal
        };
        
        order.Items.Add(item);
        
        // ❌ Temos que lembrar de recalcular o total
        RecalculateTotal(order);
    }
    
    /// <summary>
    /// Remove item do pedido
    /// ❌ Problema: Lógica de negócio fora do domínio
    /// </summary>
    public void RemoveItem(Order order, Guid itemId)
    {
        var item = order.Items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        order.Items.Remove(item);
        
        // ❌ Fácil esquecer de recalcular
        RecalculateTotal(order);
    }
    
    /// <summary>
    /// Aplica desconto
    /// ❌ Problema: Regras de negócio no serviço
    /// </summary>
    public void ApplyDiscount(Order order, decimal discountPercentage)
    {
        // ❌ Validação manual repetida
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Desconto deve estar entre 0 e 100");
        
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode aplicar desconto em pedido pendente");
        
        order.Discount = discountPercentage;
        RecalculateTotal(order);
    }
    
    /// <summary>
    /// Cancela pedido
    /// ❌ Problema: Regra de negócio espalhada
    /// </summary>
    public void CancelOrder(Order order)
    {
        // ❌ Regra de negócio no serviço
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode cancelar pedido pendente");
        
        order.Status = OrderStatus.Cancelled;
    }
    
    /// <summary>
    /// Processa o pedido
    /// </summary>
    public void ProcessOrder(Order order)
    {
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Só pode processar pedido pendente");
        
        if (!order.Items.Any())
            throw new InvalidOperationException("Pedido sem itens não pode ser processado");
        
        order.Status = OrderStatus.Processing;
    }
    
    /// <summary>
    /// Recalcula o total
    /// ❌ GRANDE PROBLEMA: Este método precisa ser chamado manualmente
    /// Se alguém esquecer, o total fica errado!
    /// </summary>
    private void RecalculateTotal(Order order)
    {
        var subtotal = order.Items.Sum(i => i.Subtotal);
        var discountAmount = subtotal * (order.Discount / 100);
        order.Total = subtotal - discountAmount;
    }
    
    /// <summary>
    /// Valida pedido
    /// ❌ Problema: Validação separada do modelo
    /// </summary>
    public bool ValidateOrder(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.CustomerName))
            return false;
        
        if (order.Items.Any(i => i.Quantity <= 0))
            return false;
        
        if (order.Items.Any(i => i.UnitPrice < 0))
            return false;
        
        return true;
    }
}

/// <summary>
/// ❌ Problemas do Domínio Anêmico:
/// 
/// 1. FALTA DE ENCAPSULAMENTO
///    - Qualquer código pode modificar qualquer propriedade
///    - Não há proteção contra estados inválidos
/// 
/// 2. LÓGICA ESPALHADA
///    - Regras de negócio estão no serviço, não no domínio
///    - Difícil encontrar onde está cada regra
/// 
/// 3. DUPLICAÇÃO
///    - Validações podem ser repetidas em vários lugares
///    - Cálculos podem ser feitos de forma diferente
/// 
/// 4. ACOPLAMENTO
///    - Serviço conhece todos os detalhes internos do domínio
///    - Mudanças no domínio afetam muitos serviços
/// 
/// 5. DIFÍCIL DE TESTAR
///    - Precisa sempre mockar serviços
///    - Não pode testar regras de negócio isoladamente
/// 
/// 6. MANUTENÇÃO
///    - Fácil esquecer de chamar métodos (como RecalculateTotal)
///    - Pode deixar objetos em estados inválidos
/// 
/// 7. EXPRESSIVIDADE
///    - Código não reflete o domínio do negócio
///    - Precisa ler serviços para entender as regras
/// </summary>
