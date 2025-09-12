# Unit of Work Pattern - Transação Atômica de Repositórios

## 📖 Visão Geral

Este projeto implementa o **Design Pattern Unit of Work** para coordenar múltiplos repositórios e garantir que todas as operações sejam aplicadas como uma única transação lógica.

## 🎯 Problema Resolvido
- **Consistência transacional** entre múltiplos repositórios
- **Evita commits parciais** em operações complexas
- **Centraliza controle de transação**

## 🏗️ Estrutura
- `UnitOfWork`: Orquestra repositórios e commit
- `CustomerRepository`, `OrderRepository`: Repositórios simulados
- `Commit()`: Persiste todas as alterações de uma vez

## 💡 Exemplo de Código

```csharp
var uow = new UnitOfWork();

var customer = new Customer { Id = 1, Name = "Lucas" };
var order = new Order { Id = 101, Description = "Pedido de livros" };

uow.Customers.Add(customer);
uow.Orders.Add(order);

// Nenhuma gravação ainda ocorreu
uow.Commit(); // Tudo é "salvo" de uma vez
```

## 🔄 Fluxo
1. Adiciona entidades aos repositórios
2. Chama `Commit()` para persistir todas as alterações
3. Se falhar, nada é persistido (simulação)

## 📝 Vantagens
- Consistência de dados
- Facilidade de rollback
- Centralização do controle

## 🚀 Execução

```bash
cd UnitOfWork
 dotnet run
```

## 🔗 Recursos
- [Unit of Work - Martin Fowler](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Documentação Microsoft](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
