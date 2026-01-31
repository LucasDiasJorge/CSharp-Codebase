# Strategy Pattern - Sistema de Pagamentos

## 📖 Visão Geral

Este projeto implementa o **Design Pattern Strategy** através de um sistema de processamento de pagamentos. O padrão Strategy define uma família de algoritmos, encapsula cada um deles e os torna intercambiáveis.

## 🎯 Problema Resolvido

O padrão Strategy resolve os seguintes problemas:
- **Condicionais complexas** - Elimina estruturas if/else ou switch extensas
- **Acoplamento forte** - Reduz dependência entre algoritmos e contexto
- **Dificuldade de extensão** - Facilita adição de novos algoritmos
- **Violação do princípio Aberto/Fechado** - Permite extensão sem modificação

## 🔧 Implementação

### Estrutura Base

```csharp
// Interface da estratégia
public interface IPagamentoStrategy
{
    void Pagar(decimal valor);
}

// Contexto que utiliza as estratégias
public class ProcessadorPagamento
{
    private IPagamentoStrategy _estrategia;

    public ProcessadorPagamento(IPagamentoStrategy estrategia)
    {
        _estrategia = estrategia;
    }

    public void SetEstrategia(IPagamentoStrategy estrategia)
    {
        _estrategia = estrategia;
    }

    public void ProcessarPagamento(decimal valor)
    {
        _estrategia.Pagar(valor);
    }
}
```

### Estratégias Implementadas

#### 1. Cartão de Crédito
```csharp
public class CartaoCreditoStrategy : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        Console.WriteLine($"Pagando R${valor} com Cartão de Crédito.");
    }
}
```

#### 2. PIX
```csharp
public class PixStrategy : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        Console.WriteLine($"Pagando R${valor} com Pix.");
    }
}
```

#### 3. Boleto
```csharp
public class BoletoStrategy : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        Console.WriteLine($"Gerando boleto para R${valor}.");
    }
}
```

## 🚀 Como Usar

### Uso Básico
```csharp
// Criando processador com estratégia inicial
var processador = new ProcessadorPagamento(new CartaoCreditoStrategy());
processador.ProcessarPagamento(150.00m);
```

### Mudança de Estratégia em Runtime
```csharp
// Alterando estratégia dinamicamente
processador.SetEstrategia(new PixStrategy());
processador.ProcessarPagamento(75.00m);

processador.SetEstrategia(new BoletoStrategy());
processador.ProcessarPagamento(200.00m);
```

## ✅ Vantagens

1. **Flexibilidade**: Algoritmos podem ser trocados em tempo de execução
2. **Extensibilidade**: Fácil adição de novas estratégias
3. **Testabilidade**: Cada estratégia pode ser testada independentemente
4. **Princípios SOLID**: Segue SRP, OCP e DIP
5. **Remoção de condicionais**: Elimina estruturas if/else complexas

## 🔄 Extensões Avançadas

### Strategy com Configuração
```csharp
public class CartaoCreditoStrategy : IPagamentoStrategy
{
    private readonly string _bandeira;
    private readonly int _parcelas;

    public CartaoCreditoStrategy(string bandeira, int parcelas = 1)
    {
        _bandeira = bandeira;
        _parcelas = parcelas;
    }

    public void Pagar(decimal valor)
    {
        var valorParcela = valor / _parcelas;
        Console.WriteLine($"Pagando {_parcelas}x de R${valorParcela:F2} no cartão {_bandeira}");
    }
}
```

### Strategy com Retorno
```csharp
public interface IPagamentoStrategy
{
    PagamentoResult Pagar(decimal valor);
}

public class PagamentoResult
{
    public bool Sucesso { get; set; }
    public string TransacaoId { get; set; }
    public string Mensagem { get; set; }
}
```

### Factory para Estratégias
```csharp
public static class PagamentoStrategyFactory
{
    public static IPagamentoStrategy CriarEstrategia(TipoPagamento tipo)
    {
        return tipo switch
        {
            TipoPagamento.CartaoCredito => new CartaoCreditoStrategy(),
            TipoPagamento.Pix => new PixStrategy(),
            TipoPagamento.Boleto => new BoletoStrategy(),
            _ => throw new ArgumentException("Tipo de pagamento não suportado")
        };
    }
}
```

## 🎯 Casos de Uso Ideais

- **Sistemas de pagamento** com múltiplas formas de pagamento
- **Algoritmos de ordenação** intercambiáveis
- **Estratégias de desconto** em e-commerce
- **Algoritmos de roteamento** em sistemas de navegação
- **Diferentes formatos de relatório** (PDF, Excel, CSV)

## ❌ Quando NÃO usar

- Quando há apenas um algoritmo
- Algoritmos que nunca mudam
- Quando o custo de abstração é maior que o benefício
- Sistemas muito simples

## 🧪 Exemplo de Teste

```csharp
[Test]
public void ProcessadorPagamento_DeveUsarEstrategiaCorreta()
{
    // Arrange
    var mockStrategy = new Mock<IPagamentoStrategy>();
    var processador = new ProcessadorPagamento(mockStrategy.Object);

    // Act
    processador.ProcessarPagamento(100m);

    // Assert
    mockStrategy.Verify(s => s.Pagar(100m), Times.Once);
}

[Test]
public void ProcessadorPagamento_DeveTrocarEstrategia()
{
    // Arrange
    var strategy1 = new Mock<IPagamentoStrategy>();
    var strategy2 = new Mock<IPagamentoStrategy>();
    var processador = new ProcessadorPagamento(strategy1.Object);

    // Act
    processador.SetEstrategia(strategy2.Object);
    processador.ProcessarPagamento(50m);

    // Assert
    strategy1.Verify(s => s.Pagar(It.IsAny<decimal>()), Times.Never);
    strategy2.Verify(s => s.Pagar(50m), Times.Once);
}
```

## 🔄 Variações do Padrão

### 1. Strategy com Template Method
```csharp
public abstract class PagamentoStrategyBase : IPagamentoStrategy
{
    public void Pagar(decimal valor)
    {
        if (ValidarPagamento(valor))
        {
            ProcessarPagamento(valor);
            RegistrarTransacao(valor);
        }
    }

    protected virtual bool ValidarPagamento(decimal valor) => valor > 0;
    protected abstract void ProcessarPagamento(decimal valor);
    protected virtual void RegistrarTransacao(decimal valor) { /* Log */ }
}
```

### 2. Strategy com Dependency Injection
```csharp
public class ProcessadorPagamento
{
    private readonly IEnumerable<IPagamentoStrategy> _strategies;

    public ProcessadorPagamento(IEnumerable<IPagamentoStrategy> strategies)
    {
        _strategies = strategies;
    }

    public void Pagar(TipoPagamento tipo, decimal valor)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Tipo == tipo);
        strategy?.Pagar(valor);
    }
}
```

## 📊 Comparação com Outros Padrões

| Aspecto | Strategy | State | Command |
|---------|----------|-------|---------|
| **Foco** | Algoritmos | Estados | Ações |
| **Troca** | Runtime | Automática | Execução |
| **Contexto** | Mantém referência | Muda estado | Encapsula comando |

## 📚 Referências

- [Strategy Pattern - Refactoring Guru](https://refactoring.guru/design-patterns/strategy)
- [Design Patterns - Gang of Four](https://en.wikipedia.org/wiki/Design_Patterns)
- [Clean Code - Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350884)
