# 🏋️ Object Calisthenics - Guia Completo com Exemplos Práticos

## 📖 O que é Object Calisthenics?

**Object Calisthenics** é um conjunto de **9 regras de programação** criadas por **Jeff Bay** no livro "The ThoughtWorks Anthology" (2008). O termo "calisthenics" vem do grego e significa "exercícios físicos" - assim como exercícios fortalecem o corpo, essas regras fortalecem seu código orientado a objetos.

O objetivo é **forçar você a pensar diferente** sobre design de código, criando software mais:
- ✅ Legível
- ✅ Manutenível  
- ✅ Testável
- ✅ Extensível
- ✅ Coeso

## 🎯 Este Projeto

Este projeto demonstra as 9 regras através de uma **API de Pedidos (Orders)** implementada de duas formas:

| Projeto | Descrição |
|---------|-----------|
| `BadOrderApi` | ❌ **Sem** aplicar Object Calisthenics - código comum com vícios |
| `GoodOrderApi` | ✅ **Com** Object Calisthenics aplicado - código limpo e bem estruturado |

---

## 📋 As 9 Regras do Object Calisthenics

### Índice
1. [Apenas Um Nível de Indentação por Método](#regra-1-apenas-um-nível-de-indentação-por-método)
2. [Não Use a Palavra-Chave ELSE](#regra-2-não-use-a-palavra-chave-else)
3. [Encapsule Todos os Primitivos e Strings](#regra-3-encapsule-todos-os-primitivos-e-strings)
4. [First Class Collections](#regra-4-first-class-collections)
5. [Um Ponto por Linha](#regra-5-um-ponto-por-linha)
6. [Não Abrevie](#regra-6-não-abrevie)
7. [Mantenha Todas as Entidades Pequenas](#regra-7-mantenha-todas-as-entidades-pequenas)
8. [Nenhuma Classe com Mais de Duas Variáveis de Instância](#regra-8-nenhuma-classe-com-mais-de-duas-variáveis-de-instância)
9. [Sem Getters/Setters/Properties](#regra-9-sem-getterssettersproperties)

---

## Regra 1: Apenas Um Nível de Indentação por Método

### 📝 Descrição
Cada método deve ter **apenas um nível de indentação**. Isso força você a extrair código para métodos menores e mais focados.

### ❓ Por que isso importa?
- Métodos com muitos níveis de indentação são difíceis de ler
- Aumenta a complexidade ciclomática
- Dificulta testes unitários
- Esconde múltiplas responsabilidades

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Services/OrderService.cs
public (bool Success, string Message, Order? Order) CreateOrder(CreateOrderRequest request)
{
    // Nível 1 - validação do request
    if (request != null)
    {
        // Nível 2 - validação do nome
        if (!string.IsNullOrEmpty(request.CustomerName))
        {
            // Nível 3 - validação do email
            if (!string.IsNullOrEmpty(request.CustomerEmail))
            {
                // Nível 4 - validação dos itens
                if (request.Items != null && request.Items.Count > 0)
                {
                    var order = new Order { ... };

                    // Nível 5 - iteração nos itens
                    foreach (var item in request.Items)
                    {
                        var product = _products.FirstOrDefault(p => p.Id == item.ProductId);
                        
                        // Nível 6 - verificação do produto
                        if (product != null)
                        {
                            // Nível 7 - verificação do estoque
                            if (product.StockQty >= item.Quantity)
                            {
                                // Nível 8 - verificação se produto ativo
                                if (product.IsActive)
                                {
                                    // Finalmente... a lógica!
                                    // 😱 8 níveis de indentação!
                                }
                                else { return (false, "...", null); }
                            }
                            else { return (false, "...", null); }
                        }
                        else { return (false, "...", null); }
                    }
                }
                else { return (false, "...", null); }
            }
            else { return (false, "...", null); }
        }
        else { return (false, "...", null); }
    }
    else { return (false, "...", null); }
}
```

**Problemas:**
- 🔴 8 níveis de indentação
- 🔴 Método gigante (+100 linhas)
- 🔴 Impossível testar partes individuais
- 🔴 Difícil de ler e entender

### ✅ GoodOrderApi - DEPOIS (Correto)

```csharp
// GoodOrderApi/Application/Services/OrderApplicationService.cs
public Result<Order> CreateOrder(CreateOrderCommand command)
{
    var validationResult = ValidateCommand(command);      // Método extraído
    if (validationResult.IsFailure)
        return Result<Order>.Failure(validationResult.Error);

    var customerInfo = CreateCustomerInfo(command);        // Método extraído
    var order = _orderRepository.CreateOrder(customerInfo);
    
    var itemsResult = AddItemsToOrder(order, command.Items); // Método extraído
    if (itemsResult.IsFailure)
        return Result<Order>.Failure(itemsResult.Error);

    ApplyDiscountIfPresent(order, command.DiscountCode);   // Método extraído
    
    return Result<Order>.Success(order);
}

// Cada método extraído tem apenas UM nível de indentação:
private Result ValidateCommand(CreateOrderCommand command)
{
    if (string.IsNullOrWhiteSpace(command.CustomerName))
        return Result.Failure("Customer name is required");

    if (string.IsNullOrWhiteSpace(command.CustomerEmail))
        return Result.Failure("Customer email is required");

    if (command.Items.Count == 0)
        return Result.Failure("Order must have at least one item");

    return Result.Success();
}

private Result AddItemsToOrder(Order order, IReadOnlyList<OrderItemCommand> items)
{
    foreach (var itemCommand in items)
    {
        var result = AddSingleItem(order, itemCommand);  // Extrai loop body
        if (result.IsFailure)
            return result;
    }
    return Result.Success();
}
```

**Benefícios:**
- ✅ Máximo 1 nível de indentação
- ✅ Métodos pequenos e focados
- ✅ Cada método é testável isoladamente
- ✅ Código auto-documentado

### 💡 Técnicas para Aplicar

1. **Extract Method**: Extraia blocos de código para métodos separados
2. **Early Return**: Use retornos antecipados em vez de aninhar ifs
3. **Decompose Conditional**: Quebre condicionais complexos em métodos
4. **Replace Loop with Pipeline**: Use LINQ em vez de loops aninhados

---

## Regra 2: Não Use a Palavra-Chave ELSE

### 📝 Descrição
Elimine completamente o uso de `else`. Isso força você a pensar em **early returns**, **polimorfismo** e **estratégias**.

### ❓ Por que isso importa?
- `else` aumenta a complexidade cognitiva
- Geralmente indica que há lógica demais em um método
- Dificulta a leitura do "caminho feliz"
- Muitos `else if` indicam necessidade de polimorfismo

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Services/OrderService.cs
public (bool Success, string Message) UpdateOrderStatus(int orderId, string newStatus)
{
    var order = _orders.FirstOrDefault(o => o.Id == orderId);
    
    if (order != null)
    {
        if (newStatus == "Confirmed")
        {
            if (order.Status == "Pending")
            {
                order.Status = newStatus;
                return (true, "Order confirmed");
            }
            else
            {
                return (false, "Order must be pending to confirm");
            }
        }
        else if (newStatus == "Shipped")
        {
            if (order.Status == "Confirmed")
            {
                if (order.IsPaid)
                {
                    order.Status = newStatus;
                    return (true, "Order shipped");
                }
                else
                {
                    return (false, "Order must be paid before shipping");
                }
            }
            else
            {
                return (false, "Order must be confirmed before shipping");
            }
        }
        else if (newStatus == "Delivered")
        {
            // ... mais else if
        }
        else
        {
            return (false, "Invalid status");
        }
    }
    else
    {
        return (false, "Order not found");
    }
}
```

**Problemas:**
- 🔴 Múltiplos níveis de if-else
- 🔴 Difícil adicionar novos status
- 🔴 Lógica de negócio espalhada
- 🔴 Violação do Open/Closed Principle

### ✅ GoodOrderApi - DEPOIS (Correto)

**Solução 1: Early Returns**
```csharp
// GoodOrderApi/Application/Services/OrderApplicationService.cs
public Result UpdateOrderStatus(int orderId, string newStatus)
{
    var order = _orderRepository.FindById(orderId);
    
    if (order is null)
        return Result.Failure("Order not found");  // Early return

    try
    {
        ExecuteStatusTransition(order, newStatus);
        return Result.Success($"Order status updated to {newStatus}");
    }
    catch (DomainException ex)
    {
        return Result.Failure(ex.Message);
    }
}
```

**Solução 2: Switch Expression (C# 8+)**
```csharp
private void ExecuteStatusTransition(Order order, string newStatus)
{
    Action transitionAction = newStatus switch
    {
        "Confirmed" => order.Confirm,
        "Shipped" => order.Ship,
        "Delivered" => order.Deliver,
        "Cancelled" => () => CancelOrder(order),
        _ => throw new DomainException($"Invalid status: {newStatus}")
    };

    transitionAction();
}
```

**Solução 3: Polimorfismo (Status como Value Object)**
```csharp
// GoodOrderApi/Domain/ValueObjects/OrderStatus.cs
public abstract class OrderStatus
{
    public abstract string Name { get; }
    public abstract bool CanTransitionTo(OrderStatus newStatus);
    public abstract bool CanBeCancelled { get; }
}

public sealed class PendingStatus : OrderStatus
{
    public override string Name => "Pending";
    public override bool CanBeCancelled => true;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return newStatus is ConfirmedStatus or CancelledStatus;
    }
}

public sealed class ConfirmedStatus : OrderStatus
{
    public override string Name => "Confirmed";
    public override bool CanBeCancelled => true;

    public override bool CanTransitionTo(OrderStatus newStatus)
    {
        return newStatus is ShippedStatus or CancelledStatus;
    }
}
```

**Benefícios:**
- ✅ Código mais linear e legível
- ✅ Fácil adicionar novos status (Open/Closed)
- ✅ Cada status conhece suas próprias regras
- ✅ Testável isoladamente

### 💡 Técnicas para Eliminar ELSE

| Técnica | Quando Usar |
|---------|-------------|
| **Early Return** | Validações no início do método |
| **Guard Clauses** | Pré-condições que retornam erro |
| **Ternary Operator** | Atribuições simples |
| **Null Coalescing** | Valores default |
| **Switch Expression** | Múltiplas condições baseadas em valor |
| **Polimorfismo** | Comportamento varia por tipo |
| **Strategy Pattern** | Algoritmos intercambiáveis |

---

## Regra 3: Encapsule Todos os Primitivos e Strings

### 📝 Descrição
Nunca use tipos primitivos (`int`, `string`, `decimal`, `bool`) diretamente. Encapsule-os em **Value Objects** com significado de domínio.

### ❓ Por que isso importa?
- Primitivos não expressam intenção
- Não podem conter validação
- Permitem valores inválidos
- Causam "Primitive Obsession" (code smell)

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Models.cs
public class Order
{
    public int Id { get; set; }
    public string CustEmail { get; set; } = string.Empty;  // String sem validação
    public decimal TotalAmt { get; set; }                   // Decimal pode ser negativo
    public string Status { get; set; } = "Pending";         // Qualquer string aceita
    public int Qty { get; set; }                            // Pode ser zero ou negativo
}

// Uso sem validação:
var order = new Order
{
    CustEmail = "email-invalido",     // Aceito! 😱
    TotalAmt = -100,                  // Aceito! 😱
    Status = "blablabla",             // Aceito! 😱
    Qty = -5                          // Aceito! 😱
};
```

**Problemas:**
- 🔴 Valores inválidos permitidos
- 🔴 Validação espalhada pelo código
- 🔴 Sem significado semântico
- 🔴 Fácil confundir parâmetros

### ✅ GoodOrderApi - DEPOIS (Correto)

```csharp
// GoodOrderApi/Domain/ValueObjects/ValueObjects.cs

/// <summary>
/// Value Object que representa um endereço de email válido.
/// </summary>
public sealed class Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        Validate(value);
        return new Email(value);
    }

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty");

        if (!value.Contains('@'))
            throw new DomainException("Email must contain @");
    }
}

/// <summary>
/// Value Object que representa uma quantidade.
/// Garante que a quantidade seja sempre positiva.
/// </summary>
public sealed class Quantity
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Create(int value)
    {
        if (value <= 0)
            throw new DomainException("Quantity must be greater than zero");
        
        return new Quantity(value);
    }

    public Quantity Add(Quantity other) => new(Value + other.Value);
    public bool IsGreaterThanOrEqual(Quantity other) => Value >= other.Value;
}

/// <summary>
/// Value Object que representa um valor monetário.
/// </summary>
public sealed class Money
{
    public decimal Amount { get; }

    public static Money Zero => new(0);
    
    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("Money cannot be negative");
        
        return new Money(Math.Round(amount, 2));
    }

    public Money Add(Money other) => new(Amount + other.Amount);
    public Money MultiplyBy(int multiplier) => new(Amount * multiplier);
    public Money ApplyDiscount(decimal percentage)
    {
        var discount = Amount * (percentage / 100);
        return new Money(Amount - discount);
    }
}
```

**Uso:**
```csharp
// Erro ao criar com valor inválido:
var email = Email.Create("invalido");        // ❌ Exceção!
var qty = Quantity.Create(-5);               // ❌ Exceção!
var money = Money.Create(-100);              // ❌ Exceção!

// Valores válidos:
var email = Email.Create("user@email.com");  // ✅ OK
var qty = Quantity.Create(5);                // ✅ OK
var total = Money.Create(100).MultiplyBy(2); // ✅ OK = $200.00
```

**Benefícios:**
- ✅ Validação automática na criação
- ✅ Impossível ter valores inválidos
- ✅ Operações específicas do domínio
- ✅ Código auto-documentado
- ✅ Type-safe (não confunde Quantity com Money)

---

## Regra 4: First Class Collections

### 📝 Descrição
Qualquer classe que contém uma coleção não deve conter outros atributos. A coleção deve ser **wrappada** em sua própria classe com comportamento específico.

### ❓ Por que isso importa?
- Coleções expostas podem ser modificadas externamente
- Lógica relacionada à coleção fica espalhada
- Não há ponto único para regras de negócio da coleção
- Viola encapsulamento

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Models.cs
public class Order
{
    public List<OrderItem> Items { get; set; } = new();  // Coleção exposta!
    // ... outros campos
}

// Problemas no uso:
var order = GetOrder();

// Qualquer um pode modificar diretamente:
order.Items.Clear();                          // 😱 Pedido sem itens
order.Items.Add(new OrderItem { Qty = -5 });  // 😱 Item inválido
order.Items = null;                           // 😱 Referência nula

// Lógica espalhada:
var total = order.Items.Sum(i => i.TotalPrice);           // Aqui
var qty = order.Items.Sum(i => i.Qty);                     // E aqui
var hasExpensive = order.Items.Any(i => i.UnitPrice > 100); // E aqui também
```

**Problemas:**
- 🔴 Coleção pode ser modificada diretamente
- 🔴 Pode receber `null`
- 🔴 Lógica de cálculo espalhada
- 🔴 Sem validação de itens

### ✅ GoodOrderApi - DEPOIS (Correto)

```csharp
// GoodOrderApi/Domain/Entities/OrderItems.cs

/// <summary>
/// First Class Collection que encapsula a lista de itens do pedido.
/// Toda lógica relacionada à coleção está aqui.
/// </summary>
public sealed class OrderItems
{
    private readonly List<OrderItem> _items;

    private OrderItems()
    {
        _items = new List<OrderItem>();
    }

    public static OrderItems Empty() => new();

    public int Count => _items.Count;
    
    public bool HasItems => _items.Count > 0;

    // Retorna cópia readonly - não pode ser modificada externamente
    public IReadOnlyList<OrderItem> ToList() => _items.AsReadOnly();

    // Retorna NOVA instância - imutabilidade
    public OrderItems Add(OrderItem item)
    {
        var newCollection = new OrderItems();
        newCollection._items.AddRange(_items);
        newCollection._items.Add(item);
        return newCollection;
    }

    // Lógica de negócio centralizada:
    public Money CalculateTotal()
    {
        return _items.Aggregate(
            Money.Zero, 
            (total, item) => total.Add(item.CalculateTotal())
        );
    }

    public int CalculateTotalQuantity()
    {
        return _items.Sum(item => item.GetQuantity().Value);
    }

    // Expõe comportamento através de método
    public void ForEach(Action<OrderItem> action)
    {
        foreach (var item in _items)
        {
            action(item);
        }
    }
}
```

**Uso:**
```csharp
// Criação segura:
var items = OrderItems.Empty();

// Adiciona item - retorna NOVA coleção (imutável)
items = items.Add(orderItem);

// Não é possível modificar diretamente:
// items.Clear();          // ❌ Não existe
// items.Add(null);        // ❌ Não existe
// items._items.Clear();   // ❌ Privado

// Lógica centralizada:
var total = items.CalculateTotal();       // ✅ Único lugar
var qty = items.CalculateTotalQuantity(); // ✅ Único lugar
```

**Benefícios:**
- ✅ Coleção protegida de modificações externas
- ✅ Lógica de negócio em um único lugar
- ✅ Imutabilidade (retorna nova instância)
- ✅ Interface focada no domínio

---

## Regra 5: Um Ponto por Linha

### 📝 Descrição
Não use mais de um ponto (`.`) por linha, exceto para fluent interfaces. Isso é conhecido como **Lei de Demeter** - "Fale apenas com seus amigos próximos".

### ❓ Por que isso importa?
- Múltiplos pontos criam acoplamento forte
- Expõe estrutura interna dos objetos
- Dificulta mudanças na estrutura
- Violação de encapsulamento

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Services/OrderService.cs

// Acessando profundamente a estrutura:
var productName = order.Items.First().Product.Name;
var customerCity = order.Customer.Address.City.Name;
var price = order.Items[0].Product.Pricing.UnitPrice;

// Múltiplas violações em uma linha:
var total = _orders
    .Where(o => o.Customer.Address.City.Name == "São Paulo")
    .SelectMany(o => o.Items)
    .Sum(i => i.Product.Pricing.UnitPrice * i.Qty);
```

**Problemas:**
- 🔴 Conhece estrutura interna de múltiplos objetos
- 🔴 Mudança em qualquer classe quebra esse código
- 🔴 Difícil de mockar em testes
- 🔴 Cria dependências transitivas

### ✅ GoodOrderApi - DEPOIS (Correto)

**Solução 1: Delegar para o objeto**
```csharp
// GoodOrderApi/Domain/Entities/Order.cs

// Em vez de: order.Financials.Items.CalculateTotal()
// O Order expõe um método que delega:
public Money GetTotal() => Financials.CalculateTotal();

// Uso:
var total = order.GetTotal();  // ✅ Um ponto apenas
```

**Solução 2: Criar métodos intermediários**
```csharp
// Em vez de: product.Info.Identification.Name
// O Product expõe diretamente:
public string GetProductName() => Info.Identification.Name;

// Uso:
var name = product.GetProductName();  // ✅ Um ponto apenas
```

**Solução 3: Value Objects que encapsulam acesso**
```csharp
// GoodOrderApi/Domain/Entities/OrderItem.cs
public class OrderItem
{
    public OrderItemDetails Details { get; }

    public Money CalculateTotal() => Details.CalculateTotal();
    public Quantity GetQuantity() => Details.Quantity;
}

// Uso:
var total = orderItem.CalculateTotal();  // ✅ Um ponto apenas
var qty = orderItem.GetQuantity();        // ✅ Um ponto apenas
```

**Solução 4: Mappers para DTOs**
```csharp
// GoodOrderApi/Api/DTOs/Mappers.cs
public static class OrderMapper
{
    public static OrderResponse ToResponse(Order order)
    {
        // Extrai acesso em um único lugar:
        var customerName = order.CustomerData.Customer.Name.ToString();
        var total = order.Financials.CalculateTotal().Amount;
        
        return new OrderResponse
        {
            CustomerName = customerName,
            Total = total
        };
    }
}
```

**Benefícios:**
- ✅ Baixo acoplamento
- ✅ Cada objeto esconde sua estrutura interna
- ✅ Fácil de refatorar
- ✅ Mais fácil de testar

---

## Regra 6: Não Abrevie

### 📝 Descrição
Use nomes completos e descritivos. **Nunca abrevie** nomes de variáveis, métodos, classes ou propriedades.

### ❓ Por que isso importa?
- Abreviações exigem contexto mental
- Diferentes pessoas abreviam diferente
- Código é lido mais vezes do que escrito
- IDE tem autocomplete - não precisa digitar tudo

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Models.cs
public class Order
{
    public string CustName { get; set; }    // Customer? Customization?
    public string CustEmail { get; set; }   // Custom? Customer?
    public string CustAddr { get; set; }    // Address? Addon?
    public decimal TotalAmt { get; set; }   // Amount? Amt?
    public DateTime CreatedDt { get; set; } // Date? DateTime?
    public string DiscCode { get; set; }    // Discount? Disconnected?
    public int Qty { get; set; }            // Quantity? Quality?
    public string ProdName { get; set; }    // Product? Production?
    public string ProdDesc { get; set; }    // Description? Descriptor?
    public string Cat { get; set; }         // Category? Catalog? Cat (animal)?
}

// Código ilegível:
var t = o.TotalAmt - o.DiscAmt;
var msg = $"Cust {o.CustName} - Qty: {o.Items.Sum(i => i.Qty)}";
```

**Problemas:**
- 🔴 Ambiguidade (Cat = Category? Catalog?)
- 🔴 Difícil para novos desenvolvedores
- 🔴 Inconsistência (às vezes Cust, às vezes Customer)
- 🔴 Erros de interpretação

### ✅ GoodOrderApi - DEPOIS (Correto)

```csharp
// GoodOrderApi/Domain/ValueObjects/ValueObjects.cs
public sealed class Email { ... }
public sealed class PhoneNumber { ... }
public sealed class PersonName { ... }
public sealed class Quantity { ... }
public sealed class Money { ... }

// GoodOrderApi/Domain/Entities/Product.cs
public sealed class ProductIdentification
{
    public string Name { get; }          // ✅ Claro
    public string Description { get; }   // ✅ Claro
}

public sealed class ProductPricing
{
    public Money Price { get; }          // ✅ Claro
    public string Category { get; }      // ✅ Claro
}

public sealed class ProductInventory
{
    public Quantity StockQuantity { get; }  // ✅ Claro
    public bool IsActive { get; }           // ✅ Claro
}

// GoodOrderApi/Domain/Entities/Order.cs
public sealed class OrderCustomerData
{
    public CustomerInfo Customer { get; }   // ✅ Claro
    public DateTime CreatedAt { get; }      // ✅ Claro
}

// Código legível:
var total = order.Financials.CalculateTotal();
var customerName = order.CustomerData.Customer.Name;
```

**Benefícios:**
- ✅ Auto-documentado
- ✅ Sem ambiguidade
- ✅ Fácil de entender para novos devs
- ✅ IDE ajuda com autocomplete

### 💡 Regras para Nomenclatura

| ❌ Evite | ✅ Use |
|----------|--------|
| `CustName` | `CustomerName` |
| `TotalAmt` | `TotalAmount` |
| `Qty` | `Quantity` |
| `Dt` | `Date` ou `DateTime` |
| `Addr` | `Address` |
| `Desc` | `Description` |
| `Prod` | `Product` |
| `Cat` | `Category` |
| `i, j, k` | `index`, `item`, `element` |
| `tmp` | Nome descritivo do conteúdo |
| `str` | Nome descritivo do conteúdo |

---

## Regra 7: Mantenha Todas as Entidades Pequenas

### 📝 Descrição
- Classes com no máximo **50 linhas**
- Métodos com no máximo **5 linhas**
- Pacotes/namespaces com no máximo **10 classes**

### ❓ Por que isso importa?
- Classes grandes têm múltiplas responsabilidades
- Difícil de testar e manter
- Aumenta acoplamento
- Dificulta reuso

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Services/OrderService.cs
// Uma classe com 280+ linhas fazendo TUDO:
public class OrderService
{
    // Criação de pedidos
    public (bool, string, Order?) CreateOrder(CreateOrderRequest request) { /* 100+ linhas */ }
    
    // Atualização de status
    public (bool, string) UpdateOrderStatus(int orderId, string newStatus) { /* 80+ linhas */ }
    
    // Processamento de pagamento
    public (bool, string) ProcessPayment(int orderId, string paymentMethod) { /* 50+ linhas */ }
    
    // Queries
    public Order? GetOrderById(int id) { ... }
    public List<Order> GetAllOrders() { ... }
    public List<Order> GetOrdersByStatus(string status) { ... }
    public List<Product> GetAllProducts() { ... }
    
    // Relatórios
    public object GenerateReport(DateTime? startDate, DateTime? endDate) { /* 40+ linhas */ }
}
```

**Problemas:**
- 🔴 280+ linhas em uma classe
- 🔴 Múltiplas responsabilidades
- 🔴 Métodos gigantes
- 🔴 Impossível testar isoladamente

### ✅ GoodOrderApi - DEPOIS (Correto)

```
GoodOrderApi/
├── Domain/
│   ├── Entities/
│   │   ├── Order.cs           (~80 linhas - ainda grande, mas coeso)
│   │   ├── OrderItem.cs       (~40 linhas)
│   │   ├── OrderItems.cs      (~45 linhas)
│   │   ├── Product.cs         (~60 linhas)
│   │   └── CustomerInfo.cs    (~35 linhas)
│   │
│   └── ValueObjects/
│       ├── ValueObjects.cs    (Money, Quantity, Email ~120 linhas)
│       ├── OrderStatus.cs     (~70 linhas)
│       ├── DiscountCode.cs    (~45 linhas)
│       ├── PaymentMethod.cs   (~40 linhas)
│       ├── Address.cs         (~45 linhas)
│       └── ContactInfo.cs     (~25 linhas)
│
└── Application/
    └── Services/
        ├── OrderApplicationService.cs  (~120 linhas)
        ├── ReportService.cs            (~80 linhas)
        └── Repositories.cs             (~50 linhas)
```

**Benefícios:**
- ✅ Classes pequenas e focadas
- ✅ Uma responsabilidade por classe
- ✅ Fácil de testar
- ✅ Fácil de encontrar código

---

## Regra 8: Nenhuma Classe com Mais de Duas Variáveis de Instância

### 📝 Descrição
Cada classe deve ter **no máximo 2 variáveis de instância**. Se precisar de mais, agrupe em novos objetos.

### ❓ Por que isso importa?
- Força alta coesão
- Cria hierarquia de objetos bem definida
- Cada objeto tem responsabilidade clara
- Facilita composição

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Models.cs
public class Order
{
    public int Id { get; set; }
    public string CustName { get; set; }      // 1
    public string CustEmail { get; set; }     // 2
    public string CustPhone { get; set; }     // 3
    public string CustAddr { get; set; }      // 4
    public string CustCity { get; set; }      // 5
    public string CustZip { get; set; }       // 6
    public List<OrderItem> Items { get; set; }// 7
    public decimal TotalAmt { get; set; }     // 8
    public string Status { get; set; }        // 9
    public DateTime CreatedDt { get; set; }   // 10
    public DateTime? ShippedDt { get; set; }  // 11
    public string PaymentMethod { get; set; } // 12
    public bool IsPaid { get; set; }          // 13
    public string DiscCode { get; set; }      // 14
    public decimal DiscAmt { get; set; }      // 15
    // 😱 15 variáveis de instância!
}
```

**Problemas:**
- 🔴 Classe God Object
- 🔴 Baixa coesão
- 🔴 Múltiplas responsabilidades
- 🔴 Difícil de entender

### ✅ GoodOrderApi - DEPOIS (Correto)

```csharp
// GoodOrderApi/Domain/Entities/Order.cs

// Order tem apenas 3 propriedades de "alto nível"
// (poderia ser 2 agrupando CustomerData e State, mas 3 é aceitável)
public class Order
{
    public int Id { get; }
    public OrderCustomerData CustomerData { get; }  // Agrupa dados do cliente
    public OrderFinancials Financials { get; }      // Agrupa dados financeiros
    public OrderState State { get; }                // Agrupa estado
}

// Cada sub-objeto também segue a regra:

public sealed class OrderCustomerData  // ✅ 2 variáveis
{
    public CustomerInfo Customer { get; }
    public DateTime CreatedAt { get; }
}

public sealed class OrderFinancials    // ✅ 2 variáveis
{
    public OrderItems Items { get; }
    public DiscountInfo Discount { get; }
}

public sealed class OrderState         // ✅ 2 variáveis
{
    public OrderStatus Status { get; }
    public PaymentInfo Payment { get; }
}

public sealed class PaymentInfo        // ✅ 2 variáveis
{
    public PaymentStatus Status { get; }
    public DateTime? ShippedAt { get; }
}

public sealed class Address            // ✅ 2 variáveis
{
    public string Street { get; }
    public CityInfo City { get; }
}

public sealed class CityInfo           // ✅ 2 variáveis
{
    public string Name { get; }
    public string ZipCode { get; }
}
```

**Estrutura Visual:**
```
Order
├── Id
├── CustomerData
│   ├── Customer (CustomerInfo)
│   │   ├── Name (PersonName)
│   │   └── Contact (CustomerContact)
│   │       ├── ContactInfo
│   │       │   ├── Email
│   │       │   └── Phone
│   │       └── Address
│   │           ├── Street
│   │           └── City
│   │               ├── Name
│   │               └── ZipCode
│   └── CreatedAt
├── Financials
│   ├── Items (OrderItems - First Class Collection)
│   └── Discount (DiscountInfo)
│       ├── Code
│       └── Amount
└── State
    ├── Status (OrderStatus)
    └── Payment (PaymentInfo)
        ├── Status
        └── ShippedAt
```

**Benefícios:**
- ✅ Cada classe é altamente coesa
- ✅ Hierarquia clara de objetos
- ✅ Fácil de entender cada parte
- ✅ Mudanças são localizadas

---

## Regra 9: Sem Getters/Setters/Properties

### 📝 Descrição
Evite expor estado interno. Em vez disso, **exponha comportamento**. Pergunte "O que este objeto pode FAZER?" em vez de "O que este objeto TEM?"

### ❓ Por que isso importa?
- Getters/Setters violam encapsulamento
- Expõem representação interna
- Lógica de negócio fica fora do objeto
- Objetos viram "sacos de dados"

### ❌ BadOrderApi - ANTES (Violação)

```csharp
// BadOrderApi/Models.cs
public class Order
{
    public string Status { get; set; }
    public bool IsPaid { get; set; }
    public decimal TotalAmt { get; set; }
    public List<OrderItem> Items { get; set; }
}

// Lógica FORA do objeto:
public void ProcessOrder(Order order)
{
    // Pegando dados e processando externamente:
    if (order.Status == "Pending" && order.IsPaid)
    {
        order.Status = "Confirmed";
        
        var total = 0m;
        foreach (var item in order.Items)
        {
            total += item.Qty * item.UnitPrice;
        }
        order.TotalAmt = total;
    }
}
```

**Problemas:**
- 🔴 Qualquer código pode modificar estado
- 🔴 Lógica de negócio espalhada
- 🔴 Objeto anêmico (sem comportamento)
- 🔴 Invariantes não são garantidas

### ✅ GoodOrderApi - DEPOIS (Correto)

```csharp
// GoodOrderApi/Domain/Entities/Order.cs
public class Order
{
    // Estado é privado ou readonly
    public OrderState State { get; private set; }
    public OrderFinancials Financials { get; private set; }

    // COMPORTAMENTO em vez de Setters:
    
    public void AddItem(OrderItem item)          // Comportamento
    {
        State.EnsureCanModify();
        Financials = Financials.AddItem(item);
    }

    public void Confirm()                        // Comportamento
    {
        var newStatus = OrderStatus.FromString("Confirmed");
        EnsureValidTransition(newStatus);
        State = State.TransitionTo(newStatus);
    }

    public void Ship()                           // Comportamento
    {
        EnsurePaymentForShipping();
        var newStatus = OrderStatus.FromString("Shipped");
        EnsureValidTransition(newStatus);
        State = State.Ship(newStatus);
    }

    public void MarkAsPaid(PaymentMethod method) // Comportamento
    {
        State = State.MarkAsPaid(method);
    }

    // Métodos internos garantem invariantes:
    private void EnsureValidTransition(OrderStatus newStatus)
    {
        if (!State.Status.CanTransitionTo(newStatus))
            throw new DomainException($"Cannot transition from {State.Status.Name} to {newStatus.Name}");
    }

    private void EnsurePaymentForShipping()
    {
        if (State.Status.RequiresPaymentForShipping && !State.IsPaid)
            throw new DomainException("Order must be paid before shipping");
    }
}

// Money com comportamento:
public sealed class Money
{
    public decimal Amount { get; }  // Apenas getter, sem setter

    // COMPORTAMENTOS:
    public Money Add(Money other) => new(Amount + other.Amount);
    public Money Subtract(Money other) => new(Amount - other.Amount);
    public Money MultiplyBy(int multiplier) => new(Amount * multiplier);
    public Money ApplyDiscount(decimal percentage) => /* ... */;
}

// Quantity com comportamento:
public sealed class Quantity
{
    public int Value { get; }

    // COMPORTAMENTOS:
    public Quantity Add(Quantity other) => new(Value + other.Value);
    public Quantity Subtract(Quantity other) => /* ... */;
    public bool IsGreaterThanOrEqual(Quantity other) => Value >= other.Value;
}
```

**Tell, Don't Ask:**
```csharp
// ❌ ANTES: Ask (pergunta estado, decide fora)
if (order.Status == "Pending" && order.IsPaid)
{
    order.Status = "Confirmed";
}

// ✅ DEPOIS: Tell (diz o que fazer)
order.Confirm();  // O Order sabe suas próprias regras
```

**Benefícios:**
- ✅ Objeto controla seu próprio estado
- ✅ Regras de negócio dentro do objeto
- ✅ Invariantes sempre garantidas
- ✅ Impossível ter estado inconsistente

---

## 🏃 Como Executar os Projetos

### Pré-requisitos
- .NET 8.0 SDK
- Visual Studio 2022 ou VS Code

### Executando BadOrderApi
```bash
cd ObjectCalisthenics/BadOrderApi
dotnet run
# Acesse: https://localhost:5001/swagger
```

### Executando GoodOrderApi
```bash
cd ObjectCalisthenics/GoodOrderApi
dotnet run
# Acesse: https://localhost:5001/swagger
```

### Testando a API
```bash
# Listar produtos
curl https://localhost:5001/api/orders/products

# Criar pedido
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "João Silva",
    "customerEmail": "joao@email.com",
    "customerPhone": "11999999999",
    "customerAddress": "Rua A, 123",
    "customerCity": "São Paulo",
    "customerZip": "01234-567",
    "items": [
      { "productId": 1, "quantity": 1 },
      { "productId": 2, "quantity": 2 }
    ],
    "discountCode": "SAVE10"
  }'

# Processar pagamento
curl -X POST https://localhost:5001/api/orders/1/payment \
  -H "Content-Type: application/json" \
  -d '{ "paymentMethod": "Pix" }'

# Atualizar status
curl -X PUT https://localhost:5001/api/orders/1/status \
  -H "Content-Type: application/json" \
  -d '{ "status": "Confirmed" }'
```

---

## 📊 Comparativo Final

| Aspecto | BadOrderApi | GoodOrderApi |
|---------|-------------|--------------|
| **Níveis de indentação** | Até 8 níveis | Máximo 1 nível |
| **Uso de ELSE** | Extensivo | Nenhum |
| **Primitivos** | Expostos | Encapsulados em Value Objects |
| **Coleções** | `List<T>` exposta | First Class Collection |
| **Pontos por linha** | Múltiplos | Um ponto por linha |
| **Nomes** | Abreviados | Completos e descritivos |
| **Tamanho das classes** | 280+ linhas | Máximo 80 linhas |
| **Variáveis de instância** | 15+ | Máximo 2 |
| **Getters/Setters** | Todos públicos | Comportamento exposto |
| **Testabilidade** | Difícil | Fácil |
| **Manutenibilidade** | Baixa | Alta |
| **Legibilidade** | Baixa | Alta |

---

## 📚 Referências

- [Object Calisthenics - Jeff Bay](https://www.cs.helsinki.fi/u/luontola/tdd-2009/ext/ObjectCalisthenics.pdf)
- [The ThoughtWorks Anthology](https://www.oreilly.com/library/view/the-thoughtworks-anthology/9781934356142/)
- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)

---

## 🤝 Contribuindo

Sinta-se à vontade para abrir issues e pull requests com melhorias ou correções!

---

**Lembre-se:** Object Calisthenics são **exercícios**, não regras absolutas. O objetivo é treinar seu cérebro para pensar em design orientado a objetos de forma mais disciplinada. Na prática, use o bom senso e adapte às necessidades do seu projeto.
