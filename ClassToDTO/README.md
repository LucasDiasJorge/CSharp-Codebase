# � ClassToDTO - Data Transfer Objects (DTOs)

## 🎯 Objetivos de Aprendizado

- Entender o padrão **Data Transfer Object (DTO)**
- Implementar transformação de **entidades de domínio** para **DTOs**
- Conhecer as vantagens dos DTOs em APIs
- Praticar mapeamento manual e automático
- Controlar exposição de dados sensíveis
- Otimizar performance reduzindo over-fetching

## 📚 Conceitos Fundamentais

### O que são DTOs?

**Data Transfer Objects (DTOs)** são objetos simples que transportam dados entre diferentes camadas da aplicação ou sistemas externos. Eles servem como contratos de dados que definem exatamente quais informações serão transferidas.

### Por que usar DTOs?

- **🔒 Segurança**: Controla quais dados são expostos
- **📡 Performance**: Reduz a quantidade de dados transferidos
- **🔄 Flexibilidade**: Permite diferentes representações da mesma entidade
- **🛡️ Isolamento**: Protege o modelo de domínio de mudanças externas
- **📋 Contratos claros**: Define interface específica para cada uso

## 🏗️ Estrutura do Projeto

```
ClassToDTO/
├── src/
│   ├── Models/          # Entidades de domínio
│   │   ├── Customer.cs
│   │   └── Order.cs
│   ├── DTO/            # Data Transfer Objects
│   │   └── OrderDTO.cs
│   ├── Controllers/    # Controladores da API
│   └── Db/            # Contexto do banco
├── Program.cs
└── README.md
```

## � Exemplos Práticos

### 1. Entidade de Domínio (Model)

```csharp
// Models/Customer.cs
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }        // Informação sensível
    public string Password { get; set; }     // Não deve ser exposta
    public DateTime CreatedAt { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

// Models/Order.cs
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
}
```

### 2. DTO (Data Transfer Object)

```csharp
// DTO/OrderDTO.cs
public class OrderDTO
{
    public int Id { get; set; }
    public string CustomerName { get; set; }  // Apenas o nome, não toda a entidade
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    // Note: sem informações sensíveis do Customer
}

// DTO/CustomerDTO.cs
public class CustomerDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Email e Password não são expostos
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}
```

### 3. Mapeamento Manual

```csharp
// Controlador ou Service
public class OrderService
{
    public OrderDTO MapToDTO(Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            CustomerName = order.Customer?.Name ?? "Unknown",
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount
        };
    }

    public List<OrderDTO> GetOrdersAsDTO()
    {
        var orders = _context.Orders
            .Include(o => o.Customer)
            .ToList();

        return orders.Select(o => MapToDTO(o)).ToList();
    }
}
```

### 4. Mapeamento com AutoMapper

```csharp
// Profile de mapeamento
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Order, OrderDTO>()
            .ForMember(dest => dest.CustomerName, 
                      opt => opt.MapFrom(src => src.Customer.Name));

        CreateMap<Customer, CustomerDTO>()
            .ForMember(dest => dest.TotalOrders, 
                      opt => opt.MapFrom(src => src.Orders.Count))
            .ForMember(dest => dest.TotalSpent, 
                      opt => opt.MapFrom(src => src.Orders.Sum(o => o.TotalAmount)));
    }
}

// Uso no controlador
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMapper _mapper;

    public OrdersController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDTO>>> GetOrders()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .ToListAsync();

        var orderDTOs = _mapper.Map<List<OrderDTO>>(orders);
        return Ok(orderDTOs);
    }
}
```

## 🚀 Configuração e Execução

### 1. Pré-requisitos

- .NET 8 SDK
- PostgreSQL instalado e rodando
- Ferramenta de cliente de banco (pgAdmin, DBeaver, etc.)

### 2. Configuração do Banco

```sql
-- Criar banco de dados
CREATE DATABASE csharp_db;

-- Tabelas serão criadas via EF Migrations
```

### 3. Executar o Projeto

```bash
# Navegar para o diretório
cd ClassToDTO

# Restaurar dependências
dotnet restore

# Aplicar migrations (se houver)
dotnet ef database update

# Executar a aplicação
dotnet run
```

### 4. Testando a API

```bash
# Listar orders como DTOs
curl -X GET https://localhost:7000/api/orders

# Criar nova order
curl -X POST https://localhost:7000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "João Silva",
    "orderDate": "2024-01-15T10:30:00",
    "totalAmount": 299.99
  }'
```

## � Padrões Avançados

### 1. DTOs para Input e Output

```csharp
// Para receber dados (Input)
public class CreateOrderDTO
{
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }
    
    public DateTime? OrderDate { get; set; } = DateTime.UtcNow;
}

// Para retornar dados (Output)
public class OrderResponseDTO
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. DTOs Hierárquicos

```csharp
public class CustomerWithOrdersDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<OrderSummaryDTO> Orders { get; set; }
}

public class OrderSummaryDTO
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
}
```

### 3. DTOs Condicionais

```csharp
public class CustomerDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // Incluído apenas para admins
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }
    
    // Incluído apenas se solicitado
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OrderDTO>? Orders { get; set; }
}
```

## 💯 Melhores Práticas

### ✅ Boas Práticas

1. **Nomes descritivos**: Use sufixo "DTO" ou prefixos como "Create", "Update"
2. **Validação**: Aplique validações nos DTOs de input
3. **Versionamento**: Mantenha diferentes versões de DTOs para compatibilidade
4. **Documentação**: Use XML docs ou Swagger para documentar DTOs
5. **Imutabilidade**: Quando possível, torne DTOs imutáveis

### ❌ Evitar

1. **Expor entidades diretamente**: Nunca retorne entidades EF nas APIs
2. **DTOs muito grandes**: Evite DTOs com muitas propriedades
3. **Lógica de negócio**: DTOs devem ser objetos "burros" de transporte
4. **Dependências**: DTOs não devem depender de outras camadas

## � Comparação: Entity vs DTO

| Aspecto | Entity (Model) | DTO |
|---------|---------------|-----|
| **Propósito** | Representa domínio | Transfere dados |
| **Localização** | Camada de domínio | Camada de apresentação |
| **Relacionamentos** | Navegação completa | Dados desnormalizados |
| **Validação** | Regras de negócio | Validação de entrada |
| **Versionamento** | Evolui com domínio | Versões específicas |
| **Exposição** | Interna | Externa (API) |

## 📋 Exercícios Práticos

1. **Criar DTOs completos**: Implemente DTOs para todas as operações CRUD
2. **Mapeamento automático**: Configure AutoMapper para todos os mapeamentos
3. **Validação avançada**: Adicione validações customizadas nos DTOs
4. **DTOs aninhados**: Crie DTOs que incluem relacionamentos
5. **Performance**: Compare queries com e sem DTOs

## 🔗 Recursos Adicionais

- [AutoMapper Documentation](https://automapper.org/)
- [System.Text.Json Guide](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview)
- [Data Transfer Object Pattern](https://martinfowler.com/eaaCatalog/dataTransferObject.html)
- [ASP.NET Core Model Binding](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)

---

💡 **Dica**: DTOs são fundamentais para APIs bem projetadas. Eles fornecem controle total sobre quais dados são expostos e como são estruturados, garantindo segurança e flexibilidade!

| Tipo         | Código exemplo                                           | Quando carrega?                  |
| ------------ | -------------------------------------------------------- | -------------------------------- |
| **Lazy**     | `var itens = pedido.Itens;`                              | Só ao acessar a propriedade      |
| **Eager**    | `.Include(p => p.Itens)`                                 | Junto com o `Pedido` na consulta |
| **Explicit** | `context.Entry(pedido).Collection(p => p.Itens).Load();` | Manualmente, sob demanda         |

---
