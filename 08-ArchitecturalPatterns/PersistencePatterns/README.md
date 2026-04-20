# Padrões de Persistência em C#

## Visão geral

Padrões de persistência são soluções arquiteturais para gerenciar como os dados são armazenados, recuperados e manipulados entre a aplicação e o banco de dados.

## Conceitos abordados

- Exemplo didático sobre Padrões de Persistência em C# no contexto de padrões arquiteturais e organização de casos de uso.
- Estrutura de código preparada para estudo, leitura rápida e execução direcionada.
- Observação prática das decisões técnicas presentes nesta implementação.

## Objetivos de aprendizagem

- Entender como Padrões de Persistência em C# se aplica em um cenário prático de padrões arquiteturais e organização de casos de uso.
- Executar o exemplo com comandos direcionados ao projeto correto.
- Usar a pasta como referência rápida para estudo e revisão posterior.

## Estrutura do projeto

```text
PersistencePatterns/
+-- Core/
|   +-- IEntity.cs
|   +-- IRepository.cs
|   \-- IUnitOfWork.cs
+-- Examples/
|   +-- IdentityMap/
|   +-- Repository/
|   \-- UnitOfWork/
+-- PersistencePatterns.csproj
\-- Program.cs
```

## Como executar

```bash
dotnet run --project 08-ArchitecturalPatterns/PersistencePatterns/PersistencePatterns.csproj
```

## Boas práticas e pontos de atenção

- Execute comandos direcionados ao arquivo .csproj mais próximo desta pasta.
- Revise dependências externas, portas e serviços auxiliares antes de rodar integrações.
- Use a documentação complementar da pasta quando o exemplo possuir cenários adicionais.

## Conteúdo complementar

##### Padrões Implementados

| Padrão | Descrição |
|--------|-----------|
| [Repository](./Examples/Repository/) | Abstração do acesso a dados |
| [Unit of Work](./Examples/UnitOfWork/) | Gerenciamento de transações |
| [Identity Map](./Examples/IdentityMap/) | Cache de entidades carregadas |
| [Data Mapper](./Examples/DataMapper/) | Separação entre domínio e persistência |
| [Active Record](./Examples/ActiveRecord/) | Entidade com métodos de persistência |

##### Estrutura de Pastas

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

##### Repository

- ✅ Abstrair acesso a dados
- ✅ Facilitar testes unitários
- ✅ Centralizar queries

##### Unit of Work

- ✅ Múltiplas operações em uma transação
- ✅ Consistência de dados
- ✅ Controle de commit/rollback

##### Identity Map

- ✅ Evitar múltiplas instâncias da mesma entidade
- ✅ Performance em operações repetidas
- ✅ Consistência em memória

##### Data Mapper

- ✅ Separar domínio de persistência
- ✅ Modelos de domínio ricos
- ✅ Independência de ORM

##### Active Record

- ✅ CRUD simples
- ✅ Prototipagem rápida
- ✅ Aplicações pequenas
