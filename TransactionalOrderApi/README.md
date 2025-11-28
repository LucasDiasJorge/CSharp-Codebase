# Transactional Order API

Projeto educacional em ASP.NET Core minimalista criado para demonstrar, de ponta a ponta, como controlar **transações** ao orquestrar fluxos de pedidos envolvendo clientes e itens. O foco é responder perguntas comuns sobre `ExecuteInTransactionAsync`, boas práticas e posicionamento do controle transacional dentro da aplicação.

## Objetivos de Aprendizado

- Centralizar o controle transacional em um **serviço de aplicação** (camada de orquestração).
- Compartilhar uma implementação simples de `ExecuteInTransactionAsync` que encapsula `BeginTransaction/Commit/Rollback`.
- Mostrar como repositories permanecem **sem estado** e não fazem `SaveChanges` nem começam transações por conta própria.
- Ilustrar uma API REST (pedidos, clientes e itens) com separação em camadas (Domain, Application, Infrastructure).
- Disponibilizar um template genérico de Unit of Work/Transaction Runner reutilizável.

## Arquitetura em Camadas

```
Api (Controllers)
  ↳ Application (Services + DTOs)  ← controla transações via ITransactionRunner
      ↳ Domain (Entities)          ← regras de negócio puras
          ↳ Infrastructure (EF Core, Repositories, Transaction Runner)
```

### Componentes-chave

| Camada | Responsabilidade | Arquivos relevantes |
| --- | --- | --- |
| API | Recebe requisições REST e delega | `Api/Controllers/OrdersController.cs` |
| Application | Orquestra casos de uso e transações | `Application/Services/OrderService.cs`, `Application/Services/EfCoreTransactionRunner.cs` |
| Domain | Modelos ricos (`Customer`, `Order`, `OrderItem`) | `Domain/Entities/*` |
| Infrastructure | EF Core + SQLite, repositories | `Infrastructure/Persistence/*`, `Infrastructure/Repositories/*` |

## Controle Transacional (ExecuteInTransactionAsync)

```csharp
public class EfCoreTransactionRunner : ITransactionRunner
{
    public async Task<T> ExecuteAsync<T>(Func<AppDbContext, CancellationToken, Task<T>> action, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            T result = await action(_context, ct);   // ← regra de negócio / repositories
            await _context.SaveChangesAsync(ct);      // ← commit unitário
            await tx.CommitAsync(ct);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
```

### Por que a transação mora na camada de serviço?

1. **Orquestração completa**: somente a camada de aplicação conhece todo o fluxo (criar cliente, registrar pedido, ajustar totais). Ela decide quando iniciar/encerrar a transação.
2. **Repositories simples**: adicionam/consultam entidades, mas não têm contexto do caso de uso. Sem transações dentro deles, evitamos aninhamentos e commits prematuros.
3. **Testabilidade**: `ITransactionRunner` é mockável. É fácil testar o caso de uso sem tocar em banco físico.
4. **Reuso**: qualquer serviço pode chamar `ExecuteInTransactionAsync`, mantendo o padrão único.

### Fluxo do endpoint `POST /api/orders`

1. Controller recebe `CreateOrderRequest` e delega para `IOrderService`.
2. `OrderService.PlaceOrderAsync` chama `_transactionRunner.ExecuteAsync`.
3. Durante a lambda, o serviço busca/cria o cliente, instancia o pedido e adiciona itens.
4. `OrderService` NÃO chama `SaveChanges`. O runner centraliza `SaveChanges` + `Commit`.
5. Em caso de falha em qualquer etapa, `Rollback` acontece automaticamente.

## Boas Práticas de Transações (aplicadas aqui)

1. **Transações curtas**: somente operações de banco necessárias entram na transação. Nada de chamadas HTTP externas ou cálculos longos.
2. **Uma entrada / uma saída**: `ExecuteInTransactionAsync` recebe uma função e devolve um resultado. Sem estado global.
3. **Evite leaks**: nunca exponha `IDbTransaction` para camadas superiores (Controller). Só serviços o conhecem.
4. **Idempotência fora da transação**: valide inputs antes de abrir a transação (`request.EnsureIsValid()`).
5. **Logging**: logue o início/fim de `ExecuteInTransactionAsync` em produção (omitido aqui por simplicidade).
6. **Cancelamento**: propague `CancellationToken` até o runner para abortar operações longas.
7. **Conexão única**: use o mesmo `DbContext`/`IDbConnection` dentro da lambda. Evita múltiplas conexões com transação aberta.

## Executando o Projeto

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Back\CSharp-101\TransactionalOrderApi"
dotnet run
```

- Swagger UI: `https://localhost:5001/swagger`
- Banco: SQLite (`transactions.db`) criado automaticamente na raiz do projeto.

### Requisições de Exemplo

**Criar pedido**
```http
POST https://localhost:5001/api/orders
Content-Type: application/json

{
  "currency": "USD",
  "customer": { "fullName": "Ada Lovelace", "email": "ada@example.com" },
  "items": [
    { "sku": "BOOK-001", "quantity": 1, "unitPrice": 49.9 },
    { "sku": "BOOK-002", "quantity": 2, "unitPrice": 35 }
  ]
}
```

**Buscar pedido**
```http
GET https://localhost:5001/api/orders/1
```

## Respondendo às Perguntas

> **Posso criar uma função `ExecuteInTransactionAsync` e colocar minhas operações dentro da lambda?**  
Sim. `EfCoreTransactionRunner` é exatamente isso. Ele inicia a transação, executa a lambda, chama `SaveChanges` uma vez e faz commit/rollback.

> **Onde devo controlar transações? No service com `BeginTransaction` e repassando para repositories?**  
Sim, a camada de serviço inicia a transação. Em EF Core, não é preciso passar o `IDbTransaction` explícito para os repositories porque todos compartilham o mesmo `DbContext`. Se usar Dapper/ADO.NET, injete o `IDbTransaction` nos repositories (veja o template abaixo).

> **Quais as melhores práticas?**  
- Abra a transação o mais tarde possível e finalize o mais cedo possível.
- Faça validações fora da transação.
- Tenha um único ponto de commit.
- Evite operações externas (HTTP, fila) dentro da transação; finalize o commit e depois publique eventos.
- Centralize a lógica em um runner/unidade de trabalho reutilizável.

## Template de Unit of Work / Transaction Runner (ADO.NET / Dapper)

Arquivo: `Templates/GenericUnitOfWork.cs`

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> action);
    Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action);
}

public sealed class GenericUnitOfWork(IDbConnection connection) : IUnitOfWork
{
    private readonly IDbConnection _connection = connection;

    public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbTransaction, Task<T>> action)
    {
        await EnsureOpenAsync();
        using var transaction = _connection.BeginTransaction();
        try
        {
            T result = await action(transaction);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action)
        => ExecuteInTransactionAsync(async tx => { await action(tx); return true; });

    private async Task EnsureOpenAsync()
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }
    }

    public ValueTask DisposeAsync()
    {
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }
}
```

Use esse template quando trabalhar com `IDbConnection` (Dapper, ADO.NET). A regra permanece: **o unit of work executa a lambda, repositories só recebem o `IDbTransaction` via parâmetro**.

## Próximos Passos

- Expandir casos de uso: cancelamento de pedidos, estorno financeiro, publicação de eventos após `Commit`.
- Adicionar testes unitários/integrados validando rollback em caso de exceções.
- Demonstrar integração com `TransactionScope` para cenários distribuídos.

---

> "Transação bem controlada é aquela que você quase esquece que existe" – mantenha seu domínio limpo e deixe o runner fazer o trabalho pesado.
