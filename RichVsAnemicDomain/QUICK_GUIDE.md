# Guia Rápido: Domínio Anêmico vs Rico

## ⚡ TL;DR

**Domínio Anêmico** = Classes só com dados, lógica em serviços ❌  
**Domínio Rico** = Classes com dados + comportamento ✅

---

## 🔄 Conversão: Anêmico → Rico

### Passo 1: Torne Propriedades Privadas

```csharp
// ❌ Antes (Anêmico)
public class Order
{
    public decimal Total { get; set; }
}

// ✅ Depois (Rico)
public class Order
{
    public decimal Total { get; private set; }
}
```

### Passo 2: Mova Lógica para o Domínio

```csharp
// ❌ Antes (Lógica no Serviço)
public class OrderService
{
    public void AddItem(Order order, string product, int qty, decimal price)
    {
        if (qty <= 0) throw new Exception("...");
        order.Items.Add(new OrderItem { ... });
    }
}

// ✅ Depois (Lógica no Domínio)
public class Order
{
    public void AddItem(string product, int qty, decimal price)
    {
        if (qty <= 0) throw new Exception("...");
        _items.Add(OrderItem.Create(product, qty, price));
    }
}
```

### Passo 3: Use Factory Methods

```csharp
// ❌ Antes
var order = new Order
{
    Id = Guid.NewGuid(),
    CustomerName = "João",
    Items = new List<OrderItem>()
};

// ✅ Depois
public class Order
{
    private Order(string customerName) { ... }
    
    public static Order Create(string customerName)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("...");
        
        return new Order(customerName);
    }
}

var order = Order.Create("João");
```

### Passo 4: Calcule Automaticamente

```csharp
// ❌ Antes (Manual)
public class Order
{
    public decimal Total { get; set; }
}

service.RecalculateTotal(order); // Tem que lembrar!

// ✅ Depois (Automático)
public class Order
{
    public decimal Total => CalculateTotal();
    
    private decimal CalculateTotal()
    {
        return _items.Sum(i => i.Subtotal);
    }
}
```

### Passo 5: Proteja Coleções

```csharp
// ❌ Antes
public class Order
{
    public List<OrderItem> Items { get; set; }
}

order.Items.Add(invalidItem); // Sem validação!

// ✅ Depois
public class Order
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(...)
    {
        // Validação aqui
        _items.Add(...);
    }
}
```

---

## 🎯 Checklist: Seu Domínio é Rico?

Use este checklist para avaliar suas classes de domínio:

### Encapsulamento
- [ ] Propriedades têm setters privados?
- [ ] Coleções são read-only publicamente?
- [ ] Construtor é privado/protected?
- [ ] Usa factory methods (Create, Build)?

### Comportamento
- [ ] Classe tem métodos além de getters/setters?
- [ ] Lógica de negócio está na classe, não em serviços?
- [ ] Validações estão na própria classe?
- [ ] Cálculos são feitos internamente?

### Invariantes
- [ ] Impossível criar objeto em estado inválido?
- [ ] Validações sempre aplicadas?
- [ ] Regras de negócio sempre respeitadas?
- [ ] Estado sempre consistente?

### Testabilidade
- [ ] Pode testar sem mocks?
- [ ] Testes são simples e diretos?
- [ ] Não precisa de infraestrutura para testar?

**Se marcou menos de 80%: Seu domínio está anêmico!** ⚠️

---

## 🚀 Receitas Práticas

### Receita 1: Entidade com Validação

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    
    private Customer(string name, Email email)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
    }
    
    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome é obrigatório");
        
        var emailVO = Email.Create(email);
        
        return new Customer(name, emailVO);
    }
}
```

### Receita 2: Valor Objeto (Value Object)

```csharp
public class Email
{
    public string Value { get; private set; }
    
    private Email(string value)
    {
        Value = value;
    }
    
    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email é obrigatório");
        
        if (!value.Contains("@"))
            throw new ArgumentException("Email inválido");
        
        return new Email(value);
    }
    
    public override bool Equals(object obj)
    {
        return obj is Email email && Value == email.Value;
    }
    
    public override int GetHashCode() => Value.GetHashCode();
}
```

### Receita 3: Agregado (Aggregate)

```csharp
public class Order // Raiz do agregado
{
    private readonly List<OrderItem> _items = new();
    
    public void AddItem(string product, int qty, decimal price)
    {
        var item = OrderItem.Create(product, qty, price);
        _items.Add(item);
    }
    
    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null)
            throw new InvalidOperationException("Item não encontrado");
        
        _items.Remove(item);
    }
}

public class OrderItem // Parte do agregado
{
    internal Guid Id { get; private set; }
    
    internal static OrderItem Create(...) { ... }
}
```

### Receita 4: Máquina de Estados

```csharp
public class Order
{
    public OrderStatus Status { get; private set; }
    
    public void Process()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                "Só pode processar pedido pendente");
        
        Status = OrderStatus.Processing;
    }
    
    public void Ship()
    {
        if (Status != OrderStatus.Processing)
            throw new InvalidOperationException(
                "Só pode enviar pedido em processamento");
        
        Status = OrderStatus.Shipped;
    }
}
```

---

## 💊 Antipadrões Comuns

### ❌ Antipadrão 1: Setters Públicos

```csharp
// NÃO faça isso!
public class Order
{
    public decimal Total { get; set; } // ❌
}

order.Total = -1000; // Aceita!

// Faça isso:
public class Order
{
    public decimal Total => CalculateTotal(); // ✅
}
```

### ❌ Antipadrão 2: Validação em Serviços

```csharp
// NÃO faça isso!
public class OrderService
{
    public void CreateOrder(string name)
    {
        if (string.IsNullOrEmpty(name)) // ❌ Validação no serviço
            throw new Exception("...");
        
        var order = new Order { Name = name };
    }
}

// Faça isso:
public class Order
{
    public static Order Create(string name)
    {
        if (string.IsNullOrEmpty(name)) // ✅ Validação no domínio
            throw new Exception("...");
        
        return new Order(name);
    }
}
```

### ❌ Antipadrão 3: Construtores Públicos Sem Validação

```csharp
// NÃO faça isso!
public class Order
{
    public Order() { } // ❌ Permite criar inválido
}

var order = new Order(); // Sem validação!

// Faça isso:
public class Order
{
    private Order(string name) { ... } // ✅ Privado
    
    public static Order Create(string name) // ✅ Factory
    {
        // Validações
        return new Order(name);
    }
}
```

### ❌ Antipadrão 4: List<T> Público

```csharp
// NÃO faça isso!
public class Order
{
    public List<OrderItem> Items { get; set; } // ❌
}

order.Items.Add(invalidItem); // Sem validação!

// Faça isso:
public class Order
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly(); // ✅
    
    public void AddItem(...)
    {
        // Validação
        _items.Add(...);
    }
}
```

---

## 🎯 Padrões de Nomenclatura

### Métodos de Comando (Modificam estado)

```csharp
public void Process()      // ✅ Verbo no imperativo
public void Ship()
public void Cancel()
public void AddItem(...)
public void ApplyDiscount(...)
```

### Métodos de Consulta (Não modificam)

```csharp
public bool CanProcess()   // ✅ Can/Is/Has
public bool IsValid()
public bool HasItems()
public decimal GetTotal()  // ✅ Get para cálculos
```

### Factory Methods

```csharp
public static Order Create(...)      // ✅ Create
public static Order CreateFrom(...) 
public static Order Build(...)
```

---

## 📖 Para Aprender Mais

1. **Comece pelo básico:**
   - Execute `AnemicDomain` e veja os problemas
   - Execute `RichDomain` e veja a solução

2. **Leia os READMEs:**
   - [README principal](README.md)
   - [README Anêmico](AnemicDomain/README.md)
   - [README Rico](RichDomain/README.md)
   - [COMPARISON](COMPARISON.md)

3. **Pratique:**
   - Pegue uma classe sua e refatore
   - Use o checklist acima
   - Aplique as receitas

4. **Estude DDD:**
   - Eric Evans - Domain-Driven Design
   - Vaughn Vernon - Implementing DDD
   - Martin Fowler - Blog sobre patterns

---

## ✨ Dica Final

> "Comece pequeno. Escolha UMA classe anêmica e refatore para rica. Veja a diferença. Depois faça outra. Em breve será natural!"

**Boa sorte! 🚀**
