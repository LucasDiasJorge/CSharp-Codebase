# Comparação: Domínio Anêmico vs Domínio Rico

## 📊 Visão Geral

Este documento compara lado a lado as duas abordagens usando o mesmo cenário: **Sistema de Pedidos (E-commerce)**.

---

## 🏗️ Estrutura de Classes

### Domínio Anêmico ❌

```csharp
// Modelo - Apenas dados
public class Order
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
}

// Serviço - Toda a lógica
public class OrderService
{
    public void AddItem(Order order, string product, int qty, decimal price)
    {
        // Validações aqui
        // Cálculos aqui
        // Regras aqui
        order.Items.Add(new OrderItem { ... });
        RecalculateTotal(order); // Tem que lembrar!
    }
}
```

### Domínio Rico ✅

```csharp
// Modelo - Dados + Comportamento
public class Order
{
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Total => CalculateTotal(); // Sempre correto!
    public OrderStatus Status { get; private set; }
    
    public void AddItem(string product, int qty, decimal price)
    {
        // Validações aqui
        // Regras aqui
        _items.Add(OrderItem.Create(product, qty, price));
        // Total recalculado automaticamente!
    }
}

// Serviço - Apenas orquestração
public class OrderApplicationService
{
    public void AddItemToOrder(Order order, ...)
    {
        order.AddItem(...); // Delega para o domínio
        // Salva, dispara eventos
    }
}
```

---

## 🔐 Encapsulamento

### Domínio Anêmico ❌

```csharp
var order = new Order();
order.CustomerName = ""; // ✗ Permitido! (inválido)
order.Total = -1000;     // ✗ Permitido! (sem sentido)
order.Status = OrderStatus.Delivered; // ✗ Pulou etapas!
order.Items.Add(new OrderItem { Quantity = -5 }); // ✗ Inválido!
```

**Problemas:**
- Sem proteção contra modificações inválidas
- Qualquer código pode quebrar as regras
- Estados inválidos são possíveis

### Domínio Rico ✅

```csharp
var order = Order.Create("João"); // ✓ Validado
// order.CustomerName = ""; // ✓ Não compila!
// order.Total = -1000;     // ✓ Não compila!
// order.Status = OrderStatus.Delivered; // ✓ Não compila!
order.AddItem("Produto", 2, 100); // ✓ Validado
```

**Vantagens:**
- Propriedades protegidas (setters privados)
- Só pode modificar via métodos validados
- Impossível criar estados inválidos

---

## 🧮 Cálculo do Total

### Domínio Anêmico ❌

```csharp
public class OrderService
{
    public void AddItem(Order order, ...)
    {
        order.Items.Add(item);
        RecalculateTotal(order); // ✗ Tem que lembrar!
    }
    
    public void RemoveItem(Order order, Guid itemId)
    {
        order.Items.Remove(item);
        RecalculateTotal(order); // ✗ Tem que lembrar!
    }
    
    private void RecalculateTotal(Order order)
    {
        order.Total = order.Items.Sum(i => i.Subtotal);
    }
}

// ✗ PROBLEMA: E se esquecer de chamar RecalculateTotal?
order.Items[0].Quantity = 10;
// Total agora está ERRADO!
```

### Domínio Rico ✅

```csharp
public class Order
{
    // ✓ Total sempre correto - calculado automaticamente!
    public decimal Total => CalculateTotal();
    
    private decimal CalculateTotal()
    {
        var subtotal = _items.Sum(i => i.Subtotal);
        var discount = subtotal * (_discountPercentage / 100);
        return subtotal - discount;
    }
}

// ✓ Impossível ficar dessincronizado!
```

---

## ✅ Validação

### Domínio Anêmico ❌

```csharp
// Validação espalhada em vários lugares
public class OrderService
{
    public void AddItem(Order order, string product, int qty, decimal price)
    {
        if (string.IsNullOrWhiteSpace(product))
            throw new ArgumentException("...");
        
        if (qty <= 0)
            throw new ArgumentException("...");
        
        // ... mais validações
    }
    
    public void ApplyDiscount(Order order, decimal discount)
    {
        if (discount < 0 || discount > 100) // ✗ Repetindo validação
            throw new ArgumentException("...");
    }
}

// ✗ E se criar o objeto diretamente?
var item = new OrderItem { Quantity = -5 }; // Nenhuma validação!
```

### Domínio Rico ✅

```csharp
// Validação centralizada no domínio
public class OrderItem
{
    public static OrderItem Create(string product, int qty, decimal price)
    {
        if (string.IsNullOrWhiteSpace(product))
            throw new ArgumentException("...");
        
        if (qty <= 0)
            throw new ArgumentException("...");
        
        if (price < 0)
            throw new ArgumentException("...");
        
        return new OrderItem(product, qty, price);
    }
}

// ✓ Impossível criar sem validar
// var item = new OrderItem(...); // Não compila! (construtor privado)
var item = OrderItem.Create("Produto", 2, 100); // Sempre válido!
```

---

## 🔄 Transição de Estados

### Domínio Anêmico ❌

```csharp
public class OrderService
{
    public void CancelOrder(Order order)
    {
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("...");
        
        order.Status = OrderStatus.Cancelled;
    }
}

// ✗ Mas posso fazer isso:
order.Status = OrderStatus.Cancelled; // Sem validação!
order.Status = OrderStatus.Delivered; // Pulou etapas!
```

### Domínio Rico ✅

```csharp
public class Order
{
    public void Cancel()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("...");
        
        Status = OrderStatus.Cancelled;
    }
    
    public void Process()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("...");
        
        Status = OrderStatus.Processing;
    }
}

// ✓ Só pode mudar via métodos
order.Cancel(); // Validado!
// order.Status = OrderStatus.Cancelled; // Não compila!
```

---

## 🧪 Testabilidade

### Domínio Anêmico ❌

```csharp
[Test]
public void Should_Calculate_Total_With_Discount()
{
    // ✗ Precisa do serviço
    var service = new OrderService();
    var order = service.CreateOrder("Cliente");
    
    service.AddItem(order, "Produto", 2, 100);
    service.ApplyDiscount(order, 10);
    
    Assert.That(order.Total, Is.EqualTo(180));
    
    // ✗ Testando o serviço, não o domínio
    // ✗ Não pode testar regras isoladamente
}
```

### Domínio Rico ✅

```csharp
[Test]
public void Should_Calculate_Total_With_Discount()
{
    // ✓ Testa o domínio diretamente
    var order = Order.Create("Cliente");
    order.AddItem("Produto", 2, 100);
    order.ApplyDiscount(10);
    
    Assert.That(order.Total, Is.EqualTo(180));
    
    // ✓ Sem mocks, sem dependências
    // ✓ Testa regra de negócio pura
}

[Test]
public void Should_Not_Allow_Cancel_After_Processing()
{
    var order = Order.Create("Cliente");
    order.AddItem("Produto", 1, 100);
    order.Process();
    
    // ✓ Testa regra de negócio
    Assert.Throws<InvalidOperationException>(() => order.Cancel());
}
```

---

## 📋 Comparação de Código Real

### Cenário: Adicionar Item ao Pedido

#### Domínio Anêmico ❌

```csharp
// 1. Criar pedido
var order = new Order
{
    Id = Guid.NewGuid(),
    CustomerName = "João",
    Items = new List<OrderItem>(),
    Total = 0,
    Status = OrderStatus.Pending
};

// 2. Adicionar item (via serviço)
var service = new OrderService();
service.AddItem(order, "Notebook", 1, 3500);

// 3. ✗ PROBLEMA: Pode modificar diretamente
order.Items[0].Quantity = 10; // Total não atualiza!
order.Total = 999999;         // Valor absurdo permitido!

// Total: 15 linhas, várias possibilidades de erro
```

#### Domínio Rico ✅

```csharp
// 1. Criar pedido
var order = Order.Create("João");

// 2. Adicionar item
order.AddItem("Notebook", 1, 3500);

// 3. ✓ Protegido contra modificações inválidas
// order.Items[0].Quantity = 10; // Não compila!
// order.Total = 999999;         // Não compila!

// Total: 3 linhas, zero possibilidades de erro
```

---

## 📊 Tabela Comparativa

| Aspecto | Domínio Anêmico ❌ | Domínio Rico ✅ |
|---------|-------------------|----------------|
| **Encapsulamento** | Fraco (tudo público) | Forte (setters privados) |
| **Validação** | Espalhada em serviços | Centralizada no domínio |
| **Lógica de Negócio** | Em serviços | No domínio |
| **Cálculos** | Manuais (esquecer = bug) | Automáticos |
| **Estados Inválidos** | Possíveis | Impossíveis |
| **Testabilidade** | Precisa mockar serviços | Testes puros |
| **Manutenibilidade** | Baixa | Alta |
| **Acoplamento** | Alto | Baixo |
| **LOC (Linhas de Código)** | Mais código | Menos código |
| **Complexidade** | Espalhada | Localizada |
| **Expressividade** | Baixa | Alta |
| **Proteção** | Nenhuma | Total |

---

## 🎯 Quando Usar Cada Um?

### Domínio Anêmico ⚠️

**Use apenas quando:**
- ✓ CRUD extremamente simples (apenas Create, Read, Update, Delete)
- ✓ Nenhuma lógica de negócio complexa
- ✓ Prototipagem rápida (descartável)
- ✓ Scripts simples de migração de dados

**Nunca use quando:**
- ✗ Tem regras de negócio importantes
- ✗ Projeto de longo prazo
- ✗ Múltiplos desenvolvedores
- ✗ Sistema crítico

### Domínio Rico ✅

**Use quando:**
- ✓ Tem lógica de negócio significativa
- ✓ Projeto de longo prazo
- ✓ Múltiplos desenvolvedores
- ✓ Regras de negócio são críticas
- ✓ Quer testabilidade máxima
- ✓ Quer manutenibilidade
- ✓ Aplicação enterprise

**Em resumo:** Use Domínio Rico por padrão, exceto em casos triviais!

---

## 🔍 Exemplo Prático: Fluxo Completo

### Domínio Anêmico ❌

```csharp
// Cliente usa
var orderService = new OrderService();

// Criar pedido
var order = orderService.CreateOrder("João");

// Adicionar itens
orderService.AddItem(order, "Notebook", 1, 3500);
orderService.AddItem(order, "Mouse", 2, 150);

// Aplicar desconto
orderService.ApplyDiscount(order, 10);

// Processar
orderService.ProcessOrder(order);

// ✗ Problemas:
// - Cliente precisa conhecer OrderService
// - Lógica espalhada
// - Difícil encontrar regras
// - Pode modificar order diretamente
```

### Domínio Rico ✅

```csharp
// Cliente usa
var order = Order.Create("João");

// Adicionar itens
order.AddItem("Notebook", 1, 3500);
order.AddItem("Mouse", 2, 150);

// Aplicar desconto
order.ApplyDiscount(10);

// Processar
order.Process();

// ✓ Vantagens:
// - Autoexplicativo
// - Lógica no domínio
// - Fácil encontrar regras
// - Protegido contra modificações
```

---

## 💡 Lições Aprendidas

### Do Domínio Anêmico ❌

1. **Falta de encapsulamento leva a bugs**
   - Modificações diretas causam inconsistências
   - Difícil garantir estado válido

2. **Lógica espalhada dificulta manutenção**
   - Precisa procurar em vários serviços
   - Duplicação de código

3. **Testabilidade comprometida**
   - Sempre precisa de mocks
   - Difícil isolar regras de negócio

### Do Domínio Rico ✅

1. **Encapsulamento previne bugs**
   - Impossível criar estados inválidos
   - Regras sempre aplicadas

2. **Lógica centralizada facilita manutenção**
   - Tudo em um lugar
   - Fácil encontrar e modificar

3. **Testabilidade máxima**
   - Testes puros sem mocks
   - Regras isoladas

---

## 🎓 Conclusão

**Domínio Rico é superior em praticamente todos os aspectos**, exceto talvez na curva de aprendizado inicial. Mas os benefícios compensam largamente:

✅ **Menos bugs** - Estados inválidos são impossíveis
✅ **Mais manutenível** - Lógica centralizada
✅ **Mais testável** - Sem necessidade de mocks
✅ **Mais expressivo** - Código reflete o negócio
✅ **Mais profissional** - Segue melhores práticas

**Recomendação:** Use Domínio Rico como padrão. Só considere Domínio Anêmico para CRUDs triviais e descartáveis.

---

## 📚 Referências

- [Martin Fowler - Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html)
- Eric Evans - Domain-Driven Design
- Vernon Vaughn - Implementing Domain-Driven Design
- Robert C. Martin - Clean Architecture

---

**Execute os exemplos e veja a diferença na prática!** 🚀
