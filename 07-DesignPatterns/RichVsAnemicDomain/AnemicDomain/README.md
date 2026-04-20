# Domínio Anêmico (Anemic Domain)

## Visão geral

Domínio Anêmico é um **anti-padrão** onde as classes de domínio são apenas "sacolas de dados" (data bags) sem comportamento significativo. Toda a lógica de negócio fica em serviços externos.

## Conceitos abordados

- Exemplo didático sobre Domínio Anêmico (Anemic Domain) no contexto de design patterns, modelagem OO e código limpo.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Domínio Anêmico (Anemic Domain) se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
AnemicDomain/
+-- Models/
|   \-- Order.cs
+-- Services/
|   \-- OrderService.cs
+-- AnemicDomain.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/RichVsAnemicDomain/AnemicDomain/AnemicDomain.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### 1. Falta de Encapsulamento

```csharp
order.CustomerName = ""; // ❌ Aceita valor inválido
order.Total = 999999;    // ❌ Pode ser manipulado diretamente
```

##### 2. Lógica Espalhada

- Cálculos estão no `OrderService`
- Validações estão no `OrderService`
- Regras de negócio estão no `OrderService`
- O modelo não sabe nada sobre suas próprias regras!

##### 3. Estado Inconsistente

```csharp
order.Items[0].Quantity = 10;
// Total NÃO é recalculado automaticamente! ❌
```

##### 4. Difícil Manutenção

- Precisa lembrar de chamar `RecalculateTotal()` manualmente
- Fácil esquecer validações
- Código duplicado em vários lugares

##### Estrutura

```
AnemicDomain/
├── Models/
│   └── Order.cs          # ❌ Apenas dados
│   └── OrderItem.cs      # ❌ Sem comportamento
├── Services/
│   └── OrderService.cs   # ❌ Toda lógica aqui
└── Program.cs
```

##### Principais Problemas

| Problema | Descrição | Exemplo |
|----------|-----------|---------|
| **Setters Públicos** | Qualquer código pode modificar | `order.Total = -1000` |
| **Sem Validação** | Aceita estados inválidos | `item.Quantity = -5` |
| **Lógica Externa** | Regras longe dos dados | `OrderService.ApplyDiscount()` |
| **Acoplamento** | Serviço conhece tudo | Service manipula internals |
| **Teste Difícil** | Sempre precisa de mocks | Não testa domínio isolado |

##### Código Problemático

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

##### O que Observar

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

##### Por que é Ruim?

> "The fundamental horror of this anti-pattern is that it's so contrary to the basic idea of object-oriented design; which is to combine data and process together."
> 
> — Martin Fowler

##### Solução

Veja o projeto **RichDomain** para a abordagem correta!

## Referências

- [Anemic Domain Model - Martin Fowler](https://martinfowler.com/bliki/AnemicDomainModel.html)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
