# Identity Map Pattern

## Descrição

O **Identity Map** garante que cada objeto seja carregado apenas uma vez, mantendo um mapa de todos os objetos carregados na sessão atual.

## Problema que Resolve

```csharp
// Sem Identity Map - duas instâncias diferentes!
var customer1 = await repo.GetByIdAsync(id);
var customer2 = await repo.GetByIdAsync(id);
customer1 == customer2 // FALSE! Objetos diferentes

// Com Identity Map - mesma instância
var customer1 = await repo.GetByIdAsync(id);
var customer2 = await repo.GetByIdAsync(id);
customer1 == customer2 // TRUE! Mesmo objeto
```

## Estrutura

```
IdentityMap/
├── Entities/
│   └── Customer.cs
├── Interfaces/
│   ├── IIdentityMap.cs
│   └── ICustomerRepository.cs
├── Implementations/
│   ├── IdentityMap.cs
│   └── CustomerRepositoryWithIdentityMap.cs
└── README.md
```

## Diagrama

```
┌─────────────────────────────────────────────────────────────┐
│                      Repository                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  GetById(1) ──▶ ┌──────────────┐                            │
│                 │ Identity Map │ ──(MISS)──▶ Database       │
│                 │   id → obj   │ ◀────────── Customer       │
│  GetById(1) ──▶ │     1 → C1   │ ──(HIT)───▶ Return C1     │
│                 └──────────────┘                            │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Benefícios

✅ **Performance** - Evita queries duplicadas  
✅ **Consistência** - Uma única instância por entidade  
✅ **Memória** - Não duplica objetos  
✅ **Integridade referencial** - Mudanças refletem em todos os lugares  

## Exemplo de Uso

```csharp
// Configuração
var identityMap = new IdentityMap<Customer, Guid>();
var repository = new CustomerRepositoryWithIdentityMap(identityMap);

// Primeiro acesso - vai ao banco
var customer1 = await repository.GetByIdAsync(customerId);
// DB Access Count: 1

// Segundo acesso - usa cache
var customer2 = await repository.GetByIdAsync(customerId);
// DB Access Count: 1 (não incrementou!)

// Mesma instância
Console.WriteLine(ReferenceEquals(customer1, customer2)); // True

// Modificação reflete em ambos
customer1.UpdateName("Novo Nome");
Console.WriteLine(customer2.Name); // "Novo Nome"
```

## Quando Usar

✅ Múltiplos acessos à mesma entidade  
✅ Necessidade de consistência em memória  
✅ Operações dentro de uma Unit of Work  
✅ Cenários com relacionamentos complexos  

## Cuidados

⚠️ **Escopo** - Deve ter escopo de request/sessão  
⚠️ **Memória** - Limpar após operação  
⚠️ **Concorrência** - Cuidado com threads  
