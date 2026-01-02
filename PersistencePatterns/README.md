# Padrões de Persistência em C#

## O que são Padrões de Persistência?

Padrões de persistência são soluções arquiteturais para gerenciar como os dados são armazenados, recuperados e manipulados entre a aplicação e o banco de dados.

## Padrões Implementados

| Padrão | Descrição |
|--------|-----------|
| [Repository](./Examples/Repository/) | Abstração do acesso a dados |
| [Unit of Work](./Examples/UnitOfWork/) | Gerenciamento de transações |
| [Identity Map](./Examples/IdentityMap/) | Cache de entidades carregadas |
| [Data Mapper](./Examples/DataMapper/) | Separação entre domínio e persistência |
| [Active Record](./Examples/ActiveRecord/) | Entidade com métodos de persistência |

## Estrutura de Pastas

```
PersistencePatterns/
├── Core/
│   └── Interfaces base
├── Examples/
│   ├── Repository/
│   ├── UnitOfWork/
│   ├── IdentityMap/
│   ├── DataMapper/
│   └── ActiveRecord/
├── Program.cs
└── README.md
```

## Quando Usar Cada Padrão

### Repository
- ✅ Abstrair acesso a dados
- ✅ Facilitar testes unitários
- ✅ Centralizar queries

### Unit of Work
- ✅ Múltiplas operações em uma transação
- ✅ Consistência de dados
- ✅ Controle de commit/rollback

### Identity Map
- ✅ Evitar múltiplas instâncias da mesma entidade
- ✅ Performance em operações repetidas
- ✅ Consistência em memória

### Data Mapper
- ✅ Separar domínio de persistência
- ✅ Modelos de domínio ricos
- ✅ Independência de ORM

### Active Record
- ✅ CRUD simples
- ✅ Prototipagem rápida
- ✅ Aplicações pequenas

## Como Executar

```bash
cd PersistencePatterns
dotnet run
```
