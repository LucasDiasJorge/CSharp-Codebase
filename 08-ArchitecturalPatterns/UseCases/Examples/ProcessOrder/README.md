# Process Order Use Case

## Visão geral

Projeto didático do CSharp-101 dedicado a Process Order Use Case, com foco em padrões arquiteturais e organização de casos de uso.

## Conceitos abordados

- Exemplo didático sobre Process Order Use Case no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Process Order Use Case se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
ProcessOrder/
+-- DTOs/
|   +-- OrderItemInput.cs
|   +-- PaymentMethod.cs
|   +-- ProcessOrderInput.cs
|   \-- ProcessOrderOutput.cs
+-- Entities/
|   +-- Customer.cs
|   +-- CustomerTier.cs
|   +-- Order.cs
|   +-- OrderItem.cs
|   +-- OrderStatus.cs
|   \-- Product.cs
+-- Interfaces/
|   +-- ICustomerRepository.cs
|   +-- IEmailService.cs
|   +-- IOrderRepository.cs
|   +-- IPaymentGateway.cs
|   \-- IProductRepository.cs
\-- ProcessOrderUseCase.cs
```

## Como executar

```
┌─────────────────────────────────────────────────────────────────┐
│                    ProcessOrderUseCase                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Validar entrada (itens não vazio)                           │
│                          ↓                                       │
│  2. Buscar dados do cliente                                     │
│                          ↓                                       │
│  3. Criar pedido                                                │
│                          ↓                                       │
│  4. BEGIN TRANSACTION                                           │
│                          ↓                                       │
│  5. Para cada item:                                             │
│      - Buscar produto                                           │
│      - Verificar disponibilidade                                │
│      - Reservar estoque                                         │
│      - Adicionar ao pedido                                      │
│                          ↓                                       │
│  6. Calcular e aplicar desconto (baseado no tier)               │
│                          ↓                                       │
│  7. Processar pagamento                                         │
│                          ↓                                       │
│  8. Confirmar pagamento no pedido                               │
│                          ↓                                       │
│  9. Persistir pedido                                            │
│                          ↓                                       │
│  10. COMMIT TRANSACTION                                         │
│                          ↓                                       │
│  11. Enviar email de confirmação                                │
│                          ↓                                       │
│  12. Retornar resultado                                         │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Descrição

Use Case completo de e-commerce que demonstra o processamento de um pedido, incluindo validação de estoque, cálculo de descontos, pagamento e notificações.

##### Sistema de Descontos por Tier

| Tier | Desconto |
|------|----------|
| Bronze | 0% |
| Silver | 5% |
| Gold | 10% |
| Platinum | 15% |

##### Estimativa de Entrega por Método de Pagamento

| Método | Dias |
|--------|------|
| Pix | 3 |
| Cartão de Crédito | 5 |
| Cartão de Débito | 5 |
| Boleto | 10 |

##### Regras de Negócio

1. **Pedido deve ter itens** - Não é permitido pedido vazio
2. **Cliente válido** - Cliente deve existir no sistema
3. **Produto disponível** - Produto deve estar ativo
4. **Estoque suficiente** - Quantidade solicitada deve estar disponível
5. **Pagamento aprovado** - Pagamento deve ser processado com sucesso

##### Dependências

| Interface | Responsabilidade |
|-----------|------------------|
| `IOrderRepository` | Persistência de pedidos |
| `IProductRepository` | Acesso a produtos e estoque |
| `ICustomerRepository` | Dados de clientes |
| `IPaymentGateway` | Processamento de pagamentos |
| `IEmailService` | Envio de notificações |
| `IUnitOfWork` | Transações de banco |

##### Exemplo de Uso

```csharp
var input = new ProcessOrderInput(
    CustomerId: customerId,
    Items: new List<OrderItemInput>
    {
        new(ProductId: product1Id, Quantity: 2),
        new(ProductId: product2Id, Quantity: 1)
    },
    ShippingAddress: "Rua Principal, 123",
    PaymentMethod: PaymentMethod.Pix
);

var result = await useCase.ExecuteAsync(input);

if (result.IsSuccess)
{
    Console.WriteLine($"Pedido: {result.Value.OrderNumber}");
    Console.WriteLine($"Total: {result.Value.TotalAmount:C}");
    Console.WriteLine($"Desconto: {result.Value.Discount:C}");
    Console.WriteLine($"Final: {result.Value.FinalAmount:C}");
    Console.WriteLine($"Entrega: {result.Value.EstimatedDelivery:d}");
}
```

##### Cenários de Teste

- ✅ Pedido com desconto Platinum
- ✅ Pedido sem desconto (Bronze)
- ❌ Produto inexistente
- ❌ Estoque insuficiente
- ❌ Falha no pagamento
- ❌ Pedido vazio
