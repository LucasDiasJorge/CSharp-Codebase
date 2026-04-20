# Unit of Work Pattern - Transação Atômica de Repositórios

## Visão geral

Este projeto implementa o **Design Pattern Unit of Work** para coordenar múltiplos repositórios e garantir que todas as operações sejam aplicadas como uma única transação lógica.

## Conceitos abordados

- **Consistência transacional** entre múltiplos repositórios
- **Evita commits parciais** em operações complexas
- **Centraliza controle de transação**

## Objetivos de aprendizagem

- Entender como Unit of Work Pattern - Transação Atômica de Repositórios se aplica em um cenário prático de design patterns, modelagem OO e código limpo.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
UnitOfWork/
+-- CustomerRepository.cs
+-- Models.cs
+-- Program.cs
\-- UnitOfWork.csproj
```

## Como executar

```bash
dotnet run --project 07-DesignPatterns/DesignPattern/Behavioral/UnitOfWork/UnitOfWork.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Estrutura

- `UnitOfWork`: Orquestra repositórios e commit
- `CustomerRepository`, `OrderRepository`: Repositórios simulados
- `Commit()`: Persiste todas as alterações de uma vez

##### Exemplo de Código

```csharp
var uow = new UnitOfWork();

var customer = new Customer { Id = 1, Name = "Lucas" };
var order = new Order { Id = 101, Description = "Pedido de livros" };

uow.Customers.Add(customer);
uow.Orders.Add(order);

// Nenhuma gravação ainda ocorreu
uow.Commit(); // Tudo é "salvo" de uma vez
```

##### Fluxo

1. Adiciona entidades aos repositórios
2. Chama `Commit()` para persistir todas as alterações
3. Se falhar, nada é persistido (simulação)

##### Vantagens

- Consistência de dados
- Facilidade de rollback
- Centralização do controle

## Referências

- [Unit of Work - Martin Fowler](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Documentação Microsoft](https://learn.microsoft.com/en-us/ef/core/saving/transactions)
