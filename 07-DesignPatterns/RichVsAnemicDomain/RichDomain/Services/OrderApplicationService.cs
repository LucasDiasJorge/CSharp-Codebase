using RichDomain.Models;

namespace RichDomain.Services;

/// <summary>
/// Serviço de Aplicação (Application Service)
/// ✅ Apenas orquestra operações, não contém lógica de negócio
/// ✅ Lógica de negócio está no domínio (Order)
/// </summary>
public class OrderApplicationService
{
    // Este serviço poderia ter dependências como repositórios, 
    // serviços de infraestrutura, etc.
    
    /// <summary>
    /// Cria e persiste um novo pedido
    /// ✅ Apenas orquestração - lógica está no domínio
    /// </summary>
    public Order CreateOrder(string customerName)
    {
        // ✅ Domínio valida e cria
        var order = Order.Create(customerName);
        
        // Aqui poderia salvar em repositório
        // await _orderRepository.SaveAsync(order);
        
        // Poderia disparar eventos de domínio
        // await _eventPublisher.PublishAsync(new OrderCreatedEvent(order));
        
        return order;
    }
    
    /// <summary>
    /// Adiciona item e persiste
    /// ✅ Serviço apenas orquestra - Order faz o trabalho
    /// </summary>
    public void AddItemToOrder(Order order, string productName, int quantity, decimal unitPrice)
    {
        // ✅ A validação e lógica estão no Order.AddItem()
        order.AddItem(productName, quantity, unitPrice);
        
        // Aqui poderia atualizar no repositório
        // await _orderRepository.UpdateAsync(order);
    }
    
    /// <summary>
    /// Processa pedido e dispara ações relacionadas
    /// ✅ Orquestração de operações
    /// </summary>
    public void ProcessOrder(Order order)
    {
        // ✅ Regra de negócio está no domínio
        order.Process();
        
        // Serviço orquestra ações relacionadas:
        // - Salvar no banco
        // - Enviar email
        // - Disparar eventos
        // - Chamar APIs externas
        
        // await _orderRepository.UpdateAsync(order);
        // await _emailService.SendOrderConfirmationAsync(order);
        // await _eventPublisher.PublishAsync(new OrderProcessedEvent(order));
    }
}

/// <summary>
/// ✅ Diferenças do Serviço no Domínio Rico:
/// 
/// 1. RESPONSABILIDADE
///    - Não contém lógica de negócio
///    - Apenas orquestra e coordena
/// 
/// 2. SIMPLICIDADE
///    - Métodos curtos e diretos
///    - Delega para o domínio
/// 
/// 3. DESACOPLAMENTO
///    - Não conhece detalhes internos do Order
///    - Usa apenas API pública do domínio
/// 
/// 4. FACILIDADE DE TESTE
///    - Testa domínio sem serviço
///    - Testa serviço mockando infraestrutura
/// 
/// 5. MANUTENÇÃO
///    - Mudanças de regra só afetam o domínio
///    - Serviço raramente precisa mudar
/// </summary>
/// 