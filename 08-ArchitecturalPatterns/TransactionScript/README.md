# Transaction Script Pattern em C#

## Visão geral

O **Transaction Script** é um padrão que organiza a lógica de negócio em procedimentos, onde cada procedimento lida com uma única requisição do usuário. É um dos padrões mais simples de arquitetura de domínio.

## Conceitos abordados

- Exemplo didático sobre Transaction Script Pattern em C# no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Transaction Script Pattern em C# se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
TransactionScript/
+-- Core/
|   +-- IDataGateway.cs
|   +-- InMemoryDataGateway.cs
|   +-- ITransactionScript.cs
|   \-- ScriptResult.cs
+-- Examples/
|   +-- CreateInvoice/
|   +-- ProcessRefund/
|   \-- TransferMoney/
+-- Program.cs
\-- TransactionScript.csproj
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/TransactionScript/TransactionScript.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Quando Usar

| ✅ Usar Quando | ❌ Evitar Quando |
|----------------|------------------|
| Lógica simples | Lógica complexa |
| CRUD básico | Muitas regras de negócio |
| Prototipagem rápida | Sistema em crescimento |
| Equipes inexperientes | Domínio rico |
| Scripts/utilitários | Alta manutenibilidade necessária |

##### Estrutura

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

##### Transaction Script

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

##### Domain Model

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

##### Benefícios

✅ **Simples** - Fácil de entender e implementar  
✅ **Direto** - Código procedural linear  
✅ **Rápido** - Desenvolvimento ágil  
✅ **Independente** - Cada script é autônomo

##### Desvantagens

⚠️ **Duplicação** - Código pode ser repetido  
⚠️ **Manutenção** - Difícil com crescimento  
⚠️ **Testabilidade** - Mais difícil de testar isoladamente
