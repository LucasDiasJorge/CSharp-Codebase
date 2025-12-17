# Domínio Anêmico (Anemic Domain)

## ⚠️ O que é Domínio Anêmico?

Domínio Anêmico é um **anti-padrão** onde as classes de domínio são apenas "sacolas de dados" (data bags) sem comportamento significativo. Toda a lógica de negócio fica em serviços externos.

## 🚨 Problemas Demonstrados

### 1. Falta de Encapsulamento
```csharp
order.CustomerName = ""; // ❌ Aceita valor inválido
order.Total = 999999;    // ❌ Pode ser manipulado diretamente
```

### 2. Lógica Espalhada
- Cálculos estão no `OrderService`
- Validações estão no `OrderService`
- Regras de negócio estão no `OrderService`
- O modelo não sabe nada sobre suas próprias regras!

### 3. Estado Inconsistente
```csharp
order.Items[0].Quantity = 10;
// Total NÃO é recalculado automaticamente! ❌
```

### 4. Difícil Manutenção
- Precisa lembrar de chamar `RecalculateTotal()` manualmente
- Fácil esquecer validações
- Código duplicado em vários lugares

## 🏃 Como Executar

```bash
cd AnemicDomain
dotnet run
```

## 📝 Estrutura

```
AnemicDomain/
├── Models/
│   └── Order.cs          # ❌ Apenas dados
│   └── OrderItem.cs      # ❌ Sem comportamento
├── Services/
│   └── OrderService.cs   # ❌ Toda lógica aqui
└── Program.cs
```

## 🎯 Principais Problemas

| Problema | Descrição | Exemplo |
|----------|-----------|---------|
| **Setters Públicos** | Qualquer código pode modificar | `order.Total = -1000` |
| **Sem Validação** | Aceita estados inválidos | `item.Quantity = -5` |
| **Lógica Externa** | Regras longe dos dados | `OrderService.ApplyDiscount()` |
| **Acoplamento** | Serviço conhece tudo | Service manipula internals |
| **Teste Difícil** | Sempre precisa de mocks | Não testa domínio isolado |

## ❌ Código Problemático

```csharp
// Modelo Anêmico - apenas dados
public class Order
{
    public decimal Total { get; set; } // ❌ Público!
    public List<OrderItem> Items { get; set; } // ❌ Sem proteção!
}

// Lógica no Serviço
public class OrderService
{
    public void AddItem(Order order, ...) // ❌ Serviço manipula tudo
    {
        // Cálculos aqui
        // Validações aqui
        // Regras aqui
    }
}
```

## 🔍 O que Observar

1. **Models/Order.cs**: 
   - Note que é apenas uma classe com propriedades
   - Sem métodos, sem comportamento
   - Tudo é público e modificável

2. **Services/OrderService.cs**:
   - Toda a lógica está aqui
   - Métodos longos com muita responsabilidade
   - Precisa conhecer todos os detalhes internos do Order

3. **Program.cs**:
   - Demonstra como é fácil criar estados inválidos
   - Mostra problemas de sincronização
   - Exemplifica falta de proteção

## 💭 Por que é Ruim?

> "The fundamental horror of this anti-pattern is that it's so contrary to the basic idea of object-oriented design; which is to combine data and process together."
> 
> — Martin Fowler

## ➡️ Solução

Veja o projeto **RichDomain** para a abordagem correta!
