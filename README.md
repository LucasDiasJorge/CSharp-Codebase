# 🎯 C# 101 - Guia Completo de Desenvolvimento .NET

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)

> 📚 **Repositório educacional** com exemplos práticos, conceitos fundamentais e guias completos para desenvolvimento C# e .NET

---

## 📋 Índice

- [📖 Sobre o Projeto](#-sobre-o-projeto)
- [🗂️ Estrutura do Repositório](#️-estrutura-do-repositório)
- [📂 Categorias Detalhadas](#-categorias-detalhadas)
- [🚀 Como Usar](#-como-usar)
- [🛠️ Comandos Essenciais](#️-comandos-essenciais)
- [🤝 Como Contribuir](#-como-contribuir)

---

## 📖 Sobre o Projeto

Este repositório contém uma coleção abrangente de exemplos práticos em **C#** e **.NET 9**, organizados em **13 categorias temáticas** para facilitar o aprendizado e consulta.

### 🎯 Objetivos
- ✅ Fornecer exemplos práticos de conceitos C#/.NET
- ✅ Demonstrar implementações de padrões de design
- ✅ Apresentar boas práticas de desenvolvimento
- ✅ Servir como referência rápida para consulta

---

## 🗂️ Estrutura do Repositório

```
CSharp-101/
├── 01-Fundamentals/           # Conceitos básicos de C#
├── 02-AsyncAndConcurrency/    # Async/Await, Threads, Tasks
├── 03-WebAPIs/                # APIs Web, gRPC, Blazor
├── 04-Authentication/         # JWT, OAuth, 2FA, Segurança
├── 05-Messaging/              # Kafka, RabbitMQ, Filas
├── 06-Caching/                # Redis, Cache patterns
├── 07-DesignPatterns/         # GoF, SOLID, Clean Code
├── 08-ArchitecturalPatterns/  # CQRS, Saga, Circuit Breaker
├── 09-Data/                   # EF, Dapper, MongoDB, SQL
├── 10-Algorithms/             # Grafos, Ordenação, Algoritmos
├── 11-Utilities/              # Serialização, PDF, Excel
├── 12-Testing/                # Benchmarks, Unit Tests
└── 13-SDKsAndLibraries/       # SDKs customizados
```

---

## 📂 Categorias Detalhadas

### 📘 01-Fundamentals
Conceitos fundamentais da linguagem C#.

| Projeto | Descrição |
|---------|-----------|
| `Course` | Exemplos básicos para iniciantes |
| `Events` | Sistema de eventos e delegates |
| `Linq` | Language Integrated Query |
| `Reflection` | Reflexão e metaprogramação |
| `LogicalOperatorsDemo` | Operadores lógicos |
| `SymbolicDelegates` | Delegates e expressões |

---

### ⚡ 02-AsyncAndConcurrency
Programação assíncrona e concorrente.

| Projeto | Descrição |
|---------|-----------|
| `Asynchronous` | Async/await básico |
| `AsyncTasksDemo` | Task.WhenAll, WhenAny |
| `AtomicOperationsDemo` | Operações atômicas |
| `Threads` | Threads e sincronização |
| `BackgroudWorker` | IHostedService |
| `JobQueueDemo` | Filas de trabalho |
| `TaskManagement` | Gerenciamento de tarefas |

---

### 🌐 03-WebAPIs
APIs Web, serviços e aplicações.

| Projeto | Descrição |
|---------|-----------|
| `MinimalApiDemo` | Minimal APIs .NET 6+ |
| `SimpleWebAPI` | API REST básica |
| `BlazorHelloWorld` | Blazor Server |
| `CustomFilterApi` | Action/Result Filters |
| `CustomMiddleware` | Middlewares customizados |
| `FluentValidationUserApi` | Validação com FluentValidation |
| `GrpcSample` | gRPC Server/Client |
| `SwaggerClientCode` | Geração de código cliente |
| `InvoiceThrottlingApi` | Rate limiting |
| `TransactionalOrderApi` | API transacional |
| `ShareableUser` | Biblioteca compartilhável |
| `WebApplication` | MVC/Razor |

---

### 🔐 04-Authentication
Autenticação, autorização e segurança.

| Projeto | Descrição |
|---------|-----------|
| `AdvancedAuthSystem` | JWT + 2FA + RBAC/ABAC |
| `Authentication/Auth` | JWT básico |
| `Authentication/OAuthApplication` | OAuth 2.0 |
| `Security/EncryptDecrypt` | Criptografia |
| `Security/SafeVault` | Cofre seguro |
| `Security/SecurityAndAuthentication` | Exemplos integrados |

---

### 📬 05-Messaging
Mensageria e streaming de eventos.

| Projeto | Descrição |
|---------|-----------|
| `Kafka/Send` | Producer Kafka |
| `Kafka/Receive` | Consumer Kafka |
| `KafkaStreamApi` | Stream processing |
| `RabbitMQ/Send` | Producer RabbitMQ |
| `RabbitMQ/Receive` | Consumer RabbitMQ |
| `QueueExample` | Filas customizadas |

---

### 💨 06-Caching
Estratégias e padrões de cache.

| Projeto | Descrição |
|---------|-----------|
| `Caching/CacheAside` | Padrão Cache-Aside |
| `Caching/CacheIncrement` | Contadores Redis |
| `Caching/CachePatterns` | 8 padrões de cache |
| `Caching/FusionCache` | Cache híbrido |
| `Caching/RedisConsoleApp` | Operações Redis |
| `Caching/RedisMySQLIntegration` | Redis + MySQL |
| `UnifiedCacheSdk` | SDK unificado |

---

### 🏗️ 07-DesignPatterns
Padrões de design e boas práticas.

| Projeto | Descrição |
|---------|-----------|
| `DesignPattern/Behavioral` | Chain of Responsibility, Mediator, State, Strategy, Visitor |
| `DesignPattern/Creational` | Builder, Factory |
| `DesignPattern/Structural` | Adapter, Composite, Decorator |
| `SOLIDExamples` | 5 princípios SOLID |
| `ObjectCalisthenics` | 9 regras de OO |
| `CodeSmells` | Code smells e soluções |
| `RichVsAnemicDomain` | DDD comparativo |
| `StrategyIntegration` | Integração Strategy |

---

### 🏛️ 08-ArchitecturalPatterns
Padrões arquiteturais avançados.

| Projeto | Descrição |
|---------|-----------|
| `CQRSDemo` | Command/Query Separation |
| `SagaPattern` | Transações distribuídas |
| `CircuitBreakerDemo` | Resiliência |
| `CarriedEvent` | Event Carried State |
| `UseCases` | Clean Architecture |
| `PersistencePatterns` | Repository, UoW |
| `TransactionPattern` | Transações |
| `TransactionScript` | Script transacional |
| `ServiceRegistration` | Dependency Injection |

---

### 💾 09-Data
Banco de dados e persistência.

| Projeto | Descrição |
|---------|-----------|
| `Data/Dapper` | Micro-ORM Dapper |
| `Data/DapperExample` | Dapper avançado |
| `Data/MongoUserApi` | MongoDB |
| `Data/Postgres` | PostgreSQL |
| `Data/MysqlExample` | MySQL |
| `Data/MoneyStorageApi` | API financeira |
| `Data/ProcedureExample` | Stored procedures |

---

### 📊 10-Algorithms
Algoritmos e estruturas de dados.

| Projeto | Descrição |
|---------|-----------|
| `GraphTraversalDemo` | BFS, DFS |
| `LoadBalancingAlgorithms` | Round Robin, etc. |
| `RealWorldBubbleSort` | Bubble Sort prático |
| `SlidingWindows` | Janela deslizante |

---

### 🛠️ 11-Utilities
Utilitários e transformação de dados.

| Projeto | Descrição |
|---------|-----------|
| `ClassToDTO` | Mapeamento DTO |
| `ClassToXml` | Serialização XML |
| `XmlBasics` | Manipulação XML |
| `Serialization` | JSON/XML/Binary |
| `CompressDecompress` | Compressão |
| `NPOIDemo` | Excel/Word |
| `PDFGenerator` | Geração PDF |
| `DictionaryMerge` | Merge de dicionários |
| `SerilogExample` | Logging estruturado |

---

### 🧪 12-Testing
Testes e benchmarks.

| Projeto | Descrição |
|---------|-----------|
| `BenchmarkTool` | Performance benchmarks |
| `OrderRuleConsole` | Console de regras com testes xUnit |

---

### 📦 13-SDKsAndLibraries
SDKs e bibliotecas customizadas.

| Projeto | Descrição |
|---------|-----------|
| `MySimpleSdk` | SDK exemplo |

---

## 🚀 Como Usar

### 📋 Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/) ou [Visual Studio](https://visualstudio.microsoft.com/)
- [Docker](https://www.docker.com/) (para Kafka/RabbitMQ/Redis)

### ⚡ Instalação

```bash
# Clonar o repositório
git clone https://github.com/LucasDiasJorge/CSharp-101.git
cd CSharp-101

# Restaurar e compilar
dotnet build CSharp-101.sln

# Executar um projeto específico
cd 01-Fundamentals/Linq
dotnet run
```

### 🐳 Docker (serviços externos)

```bash
# Kafka
cd 05-Messaging/Kafka && docker-compose up -d

# RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management

# Redis
docker run -d --name redis -p 6379:6379 redis
```

---

## 🛠️ Comandos Essenciais

```bash
# Criar projeto console
dotnet new console -n MeuProjeto

# Criar Web API
dotnet new webapi -n MinhaApi

# Adicionar pacote
dotnet add package Newtonsoft.Json

# Executar testes
dotnet test

# Build em release
dotnet build -c Release

# Publicar
dotnet publish -c Release -o ./publish
```

---

## 🤝 Como Contribuir

1. **Fork** o repositório
2. Crie uma **branch** (`git checkout -b feature/MinhaFeature`)
3. **Commit** suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. **Push** para a branch (`git push origin feature/MinhaFeature`)
5. Abra um **Pull Request**

---

## 📄 Licença

Este projeto está sob a licença MIT.

---

<div align="center">

**🚀 Feito com ❤️ para a comunidade de desenvolvedores C#/.NET**

[⭐ Deixe uma estrela](https://github.com/LucasDiasJorge/CSharp-101) • [🐛 Reporte um bug](https://github.com/LucasDiasJorge/CSharp-101/issues)

</div>
