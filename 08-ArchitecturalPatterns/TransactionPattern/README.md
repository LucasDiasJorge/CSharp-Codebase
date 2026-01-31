# Transaction Pattern - ExecuteInTransactionAsync

> **Padrão de execução transacional assíncrona para garantir atomicidade e consistência em operações de banco de dados.**

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [O Problema](#-o-problema)
- [A Solução](#-a-solução)
- [Benefícios](#-benefícios)
- [Como Usar](#-como-usar)
- [Exemplos Práticos](#-exemplos-práticos)
- [Boas Práticas](#-boas-práticas)
- [Comparação de Abordagens](#-comparação-de-abordagens)

---

## 🎯 Visão Geral

O padrão **ExecuteInTransactionAsync** encapsula a lógica de gerenciamento de transações de banco de dados, garantindo que múltiplas operações sejam executadas atomicamente (tudo ou nada) de forma assíncrona e segura.

### Código Principal

```csharp
private async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action)
{
    using IDbTransaction transaction = _repository.BeginTransaction();
    try
    {
        await action(transaction);
        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

---

## ❌ O Problema

### Código sem o padrão (problemático):

```csharp
public async Task TransferMoney(int fromAccount, int toAccount, decimal amount)
{
    // ❌ Sem transação - inconsistência possível!
    await DebitAccount(fromAccount, amount);
    
    // 💥 Se falhar aqui, o dinheiro desaparece!
    await CreditAccount(toAccount, amount);
}
```

### Problemas comuns:

1. **🔴 Inconsistência de dados**: Se a segunda operação falhar, a primeira já foi commitada
2. **🔴 Código duplicado**: Lógica de try/catch/commit/rollback repetida em todo lugar
3. **🔴 Esquecimento de Rollback**: Desenvolvedores podem esquecer de reverter em caso de erro
4. **🔴 Resource Leaks**: Transações não fechadas corretamente (sem `using`)
5. **🔴 Baixa legibilidade**: Código de infraestrutura misturado com lógica de negócio

---

## ✅ A Solução

### Código COM o padrão (correto):

```csharp
public async Task TransferMoney(int fromAccount, int toAccount, decimal amount)
{
    await ExecuteInTransactionAsync(async transaction =>
    {
        await DebitAccount(fromAccount, amount, transaction);
        await CreditAccount(toAccount, amount, transaction);
        // ✓ Se qualquer operação falhar, TUDO é revertido automaticamente
    });
}
```

---

## 🌟 Benefícios

### 1. **Atomicidade Garantida (ACID)**

Todas as operações dentro da transação são executadas como uma **unidade atômica**:
- ✅ **Tudo funciona** → Commit automático
- ❌ **Algo falha** → Rollback automático

```csharp
await ExecuteInTransactionAsync(async tx =>
{
    await Operation1(tx); // ✓
    await Operation2(tx); // ✓
    await Operation3(tx); // ❌ FALHA
    // → Rollback automático: Operation1 e Operation2 são revertidas!
});
```

### 2. **Separação de Responsabilidades (SRP)**

- **Lógica de negócio** fica dentro do lambda
- **Gerenciamento de transação** fica encapsulado no método helper

```csharp
// SEM o padrão - mistura tudo
public async Task ProcessOrder(Order order)
{
    var transaction = BeginTransaction(); // Infraestrutura
    try
    {
        await CreateOrder(order);          // Negócio
        await UpdateInventory(order);      // Negócio
        transaction.Commit();              // Infraestrutura
    }
    catch
    {
        transaction.Rollback();            // Infraestrutura
        throw;
    }
}

// COM o padrão - separado e limpo
public async Task ProcessOrder(Order order)
{
    await ExecuteInTransactionAsync(async tx =>
    {
        await CreateOrder(order, tx);      // Apenas negócio
        await UpdateInventory(order, tx);  // Apenas negócio
    });
}
```

### 3. **DRY (Don't Repeat Yourself)**

Elimina duplicação de código de gerenciamento de transação em toda a aplicação.

**Antes (código repetido 10x):**
```csharp
var tx = BeginTransaction();
try { /* ... */ tx.Commit(); } catch { tx.Rollback(); throw; }
```

**Depois (1 linha):**
```csharp
await ExecuteInTransactionAsync(async tx => { /* ... */ });
```

### 4. **Gerenciamento Automático de Recursos**

O `using` garante que a transação seja **sempre** liberada, mesmo em caso de exceção.

```csharp
using IDbTransaction transaction = BeginTransaction();
// ✓ Sempre será disposed, não importa o que aconteça
```

### 5. **Testabilidade**

Facilita testes unitários ao isolar a lógica de negócio:

```csharp
[Fact]
public async Task Transfer_ShouldRollback_WhenInsufficientFunds()
{
    // Arrange
    var service = new BankTransferService(mockConnection);
    
    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.TransferAsync(fromAccount: 1, toAccount: 2, amount: 999999)
    );
    // ✓ Rollback automático garantido
}
```

### 6. **Legibilidade e Manutenibilidade**

O código fica mais **declarativo** e **fácil de entender**:

```csharp
// Fica óbvio que tudo acontece em uma transação
await ExecuteInTransactionAsync(async tx =>
{
    await Step1(tx);
    await Step2(tx);
    await Step3(tx);
});
```

### 7. **Suporte Assíncrono Nativo**

Funciona perfeitamente com `async/await`, evitando bloqueios de threads:

```csharp
await ExecuteInTransactionAsync(async tx =>
{
    await LongRunningDatabaseOperation(tx); // Não bloqueia thread
});
```

### 8. **Tratamento Consistente de Erros**

Sempre propaga a exceção original após o rollback:

```csharp
catch
{
    transaction.Rollback();
    throw; // ✓ Preserva stack trace original
}
```

---

## 📚 Como Usar

### 1. Herdar de `BaseRepository`

```csharp
public class MyService : BaseRepository
{
    public MyService(IDbConnection connection) : base(connection) { }
    
    public async Task DoSomething()
    {
        await ExecuteInTransactionAsync(async tx =>
        {
            // Suas operações aqui
        });
    }
}
```

### 2. Passar a transação para operações filhas

```csharp
await ExecuteInTransactionAsync(async transaction =>
{
    await Operation1(transaction);
    await Operation2(transaction);
    // Todas usando a MESMA transação
});
```

---

## 🔥 Exemplos Práticos

Este projeto inclui dois exemplos completos:

### 1️⃣ **Transferência Bancária** ([BankTransferService.cs](Examples/BankTransferService.cs))

```csharp
await transferService.TransferAsync(
    fromAccountId: 1,
    toAccountId: 2,
    amount: 500m
);
```

**O que acontece:**
1. ✅ Débito da conta origem
2. ✅ Crédito na conta destino
3. ✅ Log da operação
4. ✅ **Tudo commitado** OU ❌ **Tudo revertido** se algo falhar

### 2️⃣ **Processamento de Pedido** ([OrderService.cs](Examples/OrderService.cs))

```csharp
await orderService.ProcessOrderAsync(
    customerId: 123,
    items: [
        new OrderItem(ProductId: 1, Quantity: 2, Price: 50m),
        new OrderItem(ProductId: 2, Quantity: 1, Price: 100m)
    ],
    paymentAmount: 200m
);
```

**O que acontece:**
1. ✅ Cria pedido
2. ✅ Adiciona itens
3. ✅ Atualiza estoque (decrementa)
4. ✅ Valida pagamento
5. ✅ Registra pagamento
6. ✅ **Tudo commitado** OU ❌ **Tudo revertido** se algo falhar

---

## 🎯 Boas Práticas

### ✅ FAÇA

```csharp
// ✓ Passe a transação para métodos filhos
await ExecuteInTransactionAsync(async tx =>
{
    await SaveUser(user, tx);
    await SaveAddress(address, tx);
});

// ✓ Lance exceções para acionar rollback
if (balance < amount)
    throw new InvalidOperationException("Saldo insuficiente");

// ✓ Use using para garantir dispose
using IDbTransaction transaction = BeginTransaction();
```

### ❌ NÃO FAÇA

```csharp
// ✗ NÃO chame Commit/Rollback manualmente dentro do lambda
await ExecuteInTransactionAsync(async tx =>
{
    await DoSomething(tx);
    tx.Commit(); // ✗ NÃO! O padrão já faz isso
});

// ✗ NÃO ignore a transação em métodos filhos
await ExecuteInTransactionAsync(async tx =>
{
    await SaveUser(user); // ✗ Onde está o tx?
});

// ✗ NÃO engula exceções
catch (Exception ex)
{
    Log(ex);
    // ✗ NÃO retorne sem throw - precisa do rollback!
}
```

---

## ⚖️ Comparação de Abordagens

| Aspecto | ❌ Sem Padrão | ✅ Com Padrão |
|---------|---------------|---------------|
| **Linhas de código** | 10-15 por operação | 3-5 por operação |
| **Duplicação** | Alta (copy/paste) | Zero |
| **Risco de erro** | Alto (esquecer rollback) | Baixo (automático) |
| **Legibilidade** | Baixa (mistura infraestrutura) | Alta (foca em negócio) |
| **Manutenibilidade** | Difícil (mudanças em N lugares) | Fácil (muda em 1 lugar) |
| **Testabilidade** | Complexa | Simples |
| **Atomicidade** | Não garantida | Garantida |

---

## 🚀 Executar os Exemplos

```bash
cd TransactionPattern
dotnet run
```

### Saída esperada:

```
=== Exemplo 1: Transferência Bancária ===
  - Debitado R$ 500,00 da conta 1
  + Creditado R$ 500,00 na conta 2
  ℹ Log de transferência registrado
✓ Transferência de R$ 500,00 concluída com sucesso!

=== Exemplo 2: Processamento de Pedido ===
  ✓ Pedido #4521 criado para cliente 123
    • Item adicionado: 1 x2 = R$ 100,00
    ↓ Estoque atualizado para produto 1: -2
    • Item adicionado: 2 x1 = R$ 100,00
    ↓ Estoque atualizado para produto 2: -1
  💳 Pagamento de R$ 200,00 registrado para pedido #4521
✓ Pedido #4521 processado com sucesso! Total: R$ 200,00
```

---

## 🏗️ Estrutura do Projeto

```
TransactionPattern/
├── Core/
│   ├── IRepository.cs           # Interface base
│   └── BaseRepository.cs        # Implementação do padrão
├── Examples/
│   ├── BankTransferService.cs   # Exemplo de transferência
│   └── OrderService.cs          # Exemplo de pedido
├── Program.cs                   # Demonstração
├── TransactionPattern.csproj
└── README.md                    # Este arquivo
```

---

## 📖 Conceitos Relacionados

- **ACID Transactions**: Atomicidade, Consistência, Isolamento, Durabilidade
- **Unit of Work Pattern**: Rastreia mudanças e coordena commits
- **Repository Pattern**: Abstração de acesso a dados
- **Command Pattern**: Encapsulamento de operações
- **Template Method Pattern**: Define esqueleto de algoritmo

---

## 🎓 Quando Usar

✅ **Use quando:**
- Múltiplas operações de banco devem ser atômicas
- Precisa garantir consistência de dados
- Quer evitar duplicação de código transacional
- Trabalha com operações assíncronas

❌ **Evite quando:**
- Operação única e simples (overhead desnecessário)
- Não precisa de atomicidade
- Usando ORM com Unit of Work integrado (EF Core já tem isso)

---

## 📝 Licença

Este é um projeto de exemplo educacional - use livremente! 🚀

---

**Criado com ❤️ para demonstrar boas práticas de desenvolvimento C#**
