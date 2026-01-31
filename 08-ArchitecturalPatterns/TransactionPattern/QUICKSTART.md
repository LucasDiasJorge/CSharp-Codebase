# 🚀 Quick Start - Transaction Pattern

## Executar o Projeto

```bash
cd TransactionPattern
dotnet run
```

## O Código Principal

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

## Uso Simples

```csharp
public class MyService : BaseRepository
{
    public async Task ProcessData()
    {
        await ExecuteInTransactionAsync(async tx =>
        {
            await Operation1(tx);
            await Operation2(tx);
            // Se qualquer operação falhar, TUDO é revertido!
        });
    }
}
```

## 🎯 Principais Benefícios

✅ **Atomicidade** - Tudo ou nada  
✅ **Rollback Automático** - Sem dados inconsistentes  
✅ **DRY** - Zero duplicação de código  
✅ **Código Limpo** - Negócio separado de infraestrutura  
✅ **Seguro** - `using` garante liberação de recursos  

## 📁 Arquivos Principais

- [README.md](README.md) - Documentação completa com todos os benefícios
- [Core/BaseRepository.cs](Core/BaseRepository.cs) - Implementação do padrão
- [Examples/BankTransferService.cs](Examples/BankTransferService.cs) - Exemplo transferência bancária
- [Examples/OrderService.cs](Examples/OrderService.cs) - Exemplo processamento de pedido
- [Program.cs](Program.cs) - Demonstração executável

---

**📖 Leia o [README.md](README.md) completo para entender todos os benefícios!**
