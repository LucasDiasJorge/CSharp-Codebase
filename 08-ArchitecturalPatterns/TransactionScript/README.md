# Transaction Script Pattern em C#

## O que é Transaction Script?

O **Transaction Script** é um padrão que organiza a lógica de negócio em procedimentos, onde cada procedimento lida com uma única requisição do usuário. É um dos padrões mais simples de arquitetura de domínio.

## Quando Usar

| ✅ Usar Quando | ❌ Evitar Quando |
|----------------|------------------|
| Lógica simples | Lógica complexa |
| CRUD básico | Muitas regras de negócio |
| Prototipagem rápida | Sistema em crescimento |
| Equipes inexperientes | Domínio rico |
| Scripts/utilitários | Alta manutenibilidade necessária |

## Estrutura

```
TransactionScript/
├── Core/
│   └── ITransactionScript.cs
├── Examples/
│   ├── TransferMoney/
│   ├── CreateInvoice/
│   └── ProcessRefund/
├── Program.cs
└── README.md
```

## Comparação com Domain Model

### Transaction Script
```csharp
public class TransferMoneyScript
{
    public void Execute(Guid from, Guid to, decimal amount)
    {
        var fromAccount = db.GetAccount(from);
        var toAccount = db.GetAccount(to);
        
        fromAccount.Balance -= amount;
        toAccount.Balance += amount;
        
        db.Save(fromAccount);
        db.Save(toAccount);
    }
}
```

### Domain Model
```csharp
public class Account
{
    public void TransferTo(Account destination, Money amount)
    {
        Withdraw(amount);
        destination.Deposit(amount);
    }
}
```

## Benefícios

✅ **Simples** - Fácil de entender e implementar  
✅ **Direto** - Código procedural linear  
✅ **Rápido** - Desenvolvimento ágil  
✅ **Independente** - Cada script é autônomo  

## Desvantagens

⚠️ **Duplicação** - Código pode ser repetido  
⚠️ **Manutenção** - Difícil com crescimento  
⚠️ **Testabilidade** - Mais difícil de testar isoladamente  

## Como Executar

```bash
cd TransactionScript
dotnet run
```
