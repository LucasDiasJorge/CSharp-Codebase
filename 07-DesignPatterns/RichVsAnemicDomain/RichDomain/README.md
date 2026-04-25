# Domínio Rico (Rich Domain)

## Visão geral

Domínio Rico é um padrão onde as classes de domínio encapsulam tanto **dados** quanto **comportamento**, mantendo a lógica de negócio próxima aos dados que ela manipula.

## Conceitos abordados

- **Entities**: Order tem identidade (Id)
- **Value Objects**: OrderItem poderia ser VO
- **Invariants**: Sempre em estado válido
- **Factory Methods**: Create()
- **Aggregates**: Order é raiz, Items são filhos
- **Encapsulation**: Setters privados, coleções read-only

## Objetivos de aprendizagem

- Entender como Domínio Rico (Rich Domain) se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
RichDomain/
+-- Models/
|   \-- Order.cs
+-- Services/
|   \-- OrderApplicationService.cs
+-- Program.cs
\-- RichDomain.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/RichVsAnemicDomain/RichDomain/RichDomain.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### 1. Encapsulamento Forte

```csharp
public decimal Total { get; private set; } // ✅ Setter privado
private readonly List<OrderItem> _items = new();
public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly(); // ✅ Read-only
```

##### 2. Auto-Validação

```csharp
public static Order Create(string customerName)
{
    if (string.IsNullOrWhiteSpace(customerName))
        throw new ArgumentException("Nome do cliente é obrigatório");
    
    return new Order(customerName); // ✅ Sempre válido
}
```

##### 3. Comportamento no Domínio

```csharp
public void AddItem(string productName, int quantity, decimal unitPrice)
{
    // ✅ Validações aqui
    // ✅ Regras de negócio aqui
    // ✅ Total recalculado automaticamente
}
```

##### Estrutura

```
RichDomain/
├── Models/
│   └── Order.cs              # ✅ Dados + Comportamento
│   └── OrderItem.cs          # ✅ Auto-validação
├── Services/
│   └── OrderApplicationService.cs # ✅ Apenas orquestração
└── Program.cs
```

##### Principais Vantagens

| Vantagem | Descrição | Exemplo |
|----------|-----------|---------|
| **Encapsulamento** | Propriedades protegidas | `Total { get; private set; }` |
| **Invariantes** | Sempre em estado válido | Factory methods validam |
| **Comportamento** | Lógica no lugar certo | `order.AddItem()` |
| **Cálculos** | Sempre consistentes | `Total` calculado auto |
| **Testabilidade** | Testa sem mocks | Unidade pura |

##### Código Correto

```csharp
// Modelo Rico - dados + comportamento
public class Order
{
    public decimal Total => CalculateTotal(); // ✅ Calculado!
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(...) // ✅ Comportamento no domínio
    {
        // Validações
        // Regras de negócio
        // Total recalculado automaticamente
    }
}

// Serviço - apenas orquestração
public class OrderApplicationService
{
    public void AddItemToOrder(Order order, ...)
    {
        order.AddItem(...); // ✅ Delega para o domínio
        // Salva, dispara eventos, etc.
    }
}
```

##### O que Observar

1. **Models/Order.cs**: 
   - Setters privados
   - Factory methods (Create)
   - Comportamento encapsulado
   - Validações internas
   - Total calculado automaticamente

2. **Services/OrderApplicationService.cs**:
   - Serviço simples
   - Apenas orquestra
   - Não contém lógica de negócio
   - Delega para o domínio

3. **Program.cs**:
   - Demonstra proteções
   - Impossível criar estados inválidos
   - Total sempre consistente

##### 1. Tell, Don't Ask

```csharp
// ❌ Anêmico - ASK
if (order.Status == OrderStatus.Pending)
    order.Status = OrderStatus.Processing;

// ✅ Rico - TELL
order.Process(); // Sabe como se processar
```

##### 2. Information Expert

```csharp
// ✅ Quem tem os dados, tem a lógica
public class Order
{
    public decimal Total => CalculateTotal(); // Expert em seu cálculo
}
```

##### 3. Fail Fast

```csharp
// ✅ Valida no construtor/factory
public static Order Create(string name)
{
    if (string.IsNullOrWhiteSpace(name))
        throw new ArgumentException(...); // Falha rápido!
}
```

##### Testabilidade

```csharp
[Test]
public void Should_Calculate_Total_With_Discount()
{
    // Arrange
    var order = Order.Create("Cliente");
    order.AddItem("Produto", 2, 100);
    
    // Act
    order.ApplyDiscount(10);
    
    // Assert
    Assert.That(order.Total, Is.EqualTo(180)); // 200 - 10%
    
    // ✅ Sem mocks, sem dependências, teste puro!
}
```

##### Por que é Bom?

> "The fundamental horror of [anemic domain model] is that it's so contrary to the basic idea of object-oriented design; which is to combine data and process together."
> 
> — Martin Fowler

Domínio Rico **resolve** isso mantendo dados e processos juntos!

##### Quando Usar

- ✅ Aplicações com lógica de negócio complexa
- ✅ Projetos de longo prazo
- ✅ Quando regras de negócio são críticas
- ✅ Aplicações enterprise
- ✅ Quando quer testabilidade máxima
- ✅ Quando quer manutenibilidade

##### Comparação com Anêmico

Veja o arquivo [COMPARISON.md](../COMPARISON.md) para comparação lado a lado!

## Referências

- Fowler, Martin. [Anemic Domain Model](https://martinfowler.com/bliki/AnemicDomainModel.html)
- Vernon, Vaughn. *Implementing Domain-Driven Design*. Addison-Wesley, 2013.
