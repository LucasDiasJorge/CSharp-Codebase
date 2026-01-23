# 🎯 C# 101 - Guia Completo de Desenvolvimento .NET

![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)

> 📚 **Repositório educacional** com exemplos práticos, conceitos fundamentais e guias completos para desenvolvimento C# e .NET

---

## 📋 Índice

- [📖 Sobre o Projeto](#-sobre-o-projeto)
- [🗂️ Estrutura Completa do Projeto](#️-estrutura-completa-do-projeto)
  - [🔐 Autenticação e Segurança](#-autenticação-e-segurança)
  - [💾 Banco de Dados e ORM](#-banco-de-dados-e-orm)
  - [💨 Cache e Performance](#-cache-e-performance)
  - [📬 Mensageria e Streaming](#-mensageria-e-streaming)
  - [🌐 APIs Web e Serviços](#-apis-web-e-serviços)
  - [🏗️ Padrões de Design](#️-padrões-de-design-design-patterns)
  - [🏛️ Padrões Arquiteturais Avançados](#️-padrões-arquiteturais-avançados)
  - [🔄 Assincronia e Concorrência](#-assincronia-e-concorrência)
  - [📊 Algoritmos e Estruturas de Dados](#-algoritmos-e-estruturas-de-dados)
  - [🛠️ Utilitários e Transformação de Dados](#️-utilitários-e-transformação-de-dados)
  - [⚙️ Serviços e Infraestrutura](#️-serviços-e-infraestrutura)
  - [📦 SDKs e Bibliotecas](#-sdks-e-bibliotecas)
  - [🎓 Aprendizado e Exemplos Básicos](#-aprendizado-e-exemplos-básicos)
  - [🧪 Testes](#-testes)
- [🚀 Como Usar](#-como-usar)
- [🛠️ Comandos Essenciais do .NET CLI](#️-comandos-essenciais-do-net-cli)
- [🏗️ Princípios SOLID](#️-princípios-solid)
- [🤝 Como Contribuir](#-como-contribuir)

---

## 📖 Sobre o Projeto

Este repositório contém uma coleção abrangente de exemplos práticos em **C#** e **.NET 9**, organizados em categorias temáticas para facilitar o aprendizado e consulta de diferentes conceitos, padrões de design e técnicas de programação.

### 🎯 Objetivos
- ✅ Fornecer exemplos práticos de conceitos C#/.NET
- ✅ Demonstrar implementações de padrões de design
- ✅ Apresentar boas práticas de desenvolvimento
- ✅ Servir como referência rápida para comandos e configurações

---

## 🗂️ Estrutura Completa do Projeto

### 🔐 Autenticação e Segurança

#### `AdvancedAuthSystem/`
Sistema avançado de autenticação com JWT, incluindo:
- Controllers de autenticação e recursos
- Serviços de token e hash de senha
- Handlers de autorização customizados
- Políticas e requisitos de segurança
- DbContext com Entity Framework

#### `Authentication/`
| Projeto | Descrição |
|---------|-----------|
| `Auth/` | Implementação básica de autenticação JWT |
| `OAuthApplication/` | Aplicação OAuth com fluxo completo de autorização |

#### `Security/`
| Projeto | Descrição |
|---------|-----------|
| `EncryptDecrypt/` | Demonstração de criptografia e descriptografia |
| `SafeVault/` | Cofre seguro para armazenamento de dados sensíveis |
| `SecurityAndAuthentication/` | Exemplos integrados de segurança |

---

### 💾 Banco de Dados e ORM

#### `Data/`
| Projeto | Descrição |
|---------|-----------|
| `Dapper/` | Uso do Dapper como micro-ORM |
| `DapperExample/` | Exemplos práticos avançados com Dapper |
| `MoneyStorageApi/` | API de armazenamento financeiro |
| `MongoUserApi/` | API REST com MongoDB |
| `MysqlExample/` | Conexão e operações com MySQL |
| `Postgres/` | Conexão e operações com PostgreSQL |
| `ProcedureExample/` | Uso de stored procedures |

---

### 💨 Cache e Performance

#### `Caching/`
| Projeto | Descrição |
|---------|-----------|
| `CacheAside/` | Implementação do padrão Cache-Aside |
| `CacheIncrement/` | Incremento atômico em cache |
| `CachePatterns/` | Padrões de cache (Write-Through, Write-Behind, Near Cache, Tiered Cache) |
| `FusionCache/` | Cache híbrido com FusionCache |
| `RedisConsoleApp/` | Exemplo de uso do Redis em console |
| `RedisMySQLIntegration/` | Integração Redis + MySQL |

#### `UnifiedCacheSdk/`
SDK unificado para múltiplas estratégias de cache com abstração de providers.

---

### 📬 Mensageria e Streaming

#### `Kafka/`
| Projeto | Descrição |
|---------|-----------|
| `Send/` | Producer Kafka para envio de mensagens |
| `Receive/` | Consumer Kafka para recebimento de mensagens |
| `docker-compose.yml` | Configuração Docker para ambiente Kafka |

#### `KafkaStreamApi/`
API de stream processing com Apache Kafka, incluindo controllers e services dedicados.

#### `RabbitMQ/`
| Projeto | Descrição |
|---------|-----------|
| `Send/` | Producer RabbitMQ |
| `Receive/` | Consumer RabbitMQ |

#### `QueueExample/`
Implementação de filas personalizadas em C#.

---

### 🌐 APIs Web e Serviços

#### `SimpleWebAPI/`
API Web básica demonstrando estrutura fundamental de uma Web API.

#### `MinimalApiDemo/`
APIs mínimas do .NET 6+ com endpoints simplificados.

#### `WebApplication/`
Aplicação web completa com MVC/Razor.

#### `BlazorHelloWorld/`
Aplicação Blazor Server demonstrando conceitos básicos:
- Criação de componentes .razor
- Data binding e manipulação de eventos
- Renderização condicional
- Interatividade do lado do servidor

#### `CustomFilterApi/`
Filtros personalizados (equivalente a interceptors do Java):
- Action Filters
- Result Filters
- Exception Filters
- Atributos customizados

#### `CustomMiddleware/`
Implementação de middlewares customizados no pipeline ASP.NET Core.

#### `FluentValidationUserApi/`
Validação de modelos com FluentValidation:
- Validadores customizados
- Regras de validação complexas
- Integração com ASP.NET Core

#### `SwaggerClientCode/`
Geração automática de código cliente a partir de especificações Swagger/OpenAPI.

#### `GrpcSample/`
| Projeto | Descrição |
|---------|-----------|
| `src/GrpcSample.Server/` | Servidor gRPC |
| `src/GrpcSample.Client/` | Cliente gRPC |
| `src/GrpcSample.Contracts/` | Contratos .proto compartilhados |

#### `InvoiceThrottlingApi/`
API com rate limiting e throttling para geração de invoices:
- Controle de taxa de requisições
- Geração e processamento de notas fiscais

#### `TransactionalOrderApi/`
API com controle transacional completo:
- Domain Layer
- Application Layer
- Infrastructure Layer
- Templates de documentos

#### `ShareableUser/`
Biblioteca compartilhável de usuários com middleware customizado.

---

### 🏗️ Padrões de Design (Design Patterns)

#### `DesignPattern/`

##### Behavioral (Comportamentais)
| Padrão | Descrição |
|--------|-----------|
| `ChainOfResponsibility/` | Cadeia de responsabilidade para processamento em pipeline |
| `Mediator/` | Mediador para comunicação desacoplada |
| `State/` | Máquina de estados para comportamento contextual |
| `Strategy/` | Estratégia para algoritmos intercambiáveis |
| `UnitOfWork/` | Unidade de trabalho para transações |
| `Visitor/` | Visitante para operações em estruturas |

##### Creational (Criacionais)
| Padrão | Descrição |
|--------|-----------|
| `Builder/` | Construtor para objetos complexos |
| `Factory/` | Fábrica para criação de objetos |

##### Structural (Estruturais)
| Padrão | Descrição |
|--------|-----------|
| `Adapter/` | Adaptador para interfaces incompatíveis |
| `Composite/` | Composição para estruturas hierárquicas |
| `Decorator/` | Adiciona comportamentos a objetos dinamicamente |

#### `SOLIDExamples/`
Exemplos práticos dos 5 princípios SOLID:
| Princípio | Pasta | Descrição |
|-----------|-------|-----------|
| SRP | `SRP/` | Single Responsibility Principle |
| OCP | `OCP/` | Open/Closed Principle |
| LSP | `LSP/` | Liskov Substitution Principle |
| ISP | `ISP/` | Interface Segregation Principle |
| DIP | `DIP/` | Dependency Inversion Principle |

#### `StrategyIntegration/`
Integração prática do padrão Strategy com múltiplas classes de integração.

#### `CodeSmells/`
| Projeto | Descrição |
|---------|-----------|
| `PoisonLooping/` | Exemplos de loops problemáticos e soluções |

#### `RichVsAnemicDomain/`
Comparação entre modelos de domínio:
| Projeto | Descrição |
|---------|-----------|
| `RichDomain/` | Modelo de domínio rico (DDD) |
| `AnemicDomain/` | Modelo de domínio anêmico |
| `COMPARISON.md` | Comparativo detalhado |
| `QUICK_GUIDE.md` | Guia rápido de referência |

---

### 🏛️ Padrões Arquiteturais Avançados

#### `CQRSDemo/`
Command Query Responsibility Segregation:
- Separação de operações de leitura (Queries) e escrita (Commands)
- Commands para modificações de estado
- Queries para leituras otimizadas
- Handlers dedicados para cada operação
- Event Sourcing opcional

#### `SagaPattern/`
Padrão Saga para transações distribuídas em microserviços:
- **Orchestration**: Orquestrador central coordena todos os passos
- **Choreography**: Serviços reagem a eventos (event-driven)
- Compensating transactions para rollback
- Gerenciamento de estado da saga
- Exemplos: OrderSaga com múltiplos serviços

#### `CircuitBreakerDemo/`
Implementação do padrão Circuit Breaker para resiliência:
- **Estados**: Closed (normal), Open (falha), Half-Open (teste)
- Prevenção de falhas em cascata
- Fast-fail para evitar timeouts desnecessários
- Auto-recuperação e retry automático
- Proteção de sistemas distribuídos

#### `CarriedEvent/`
Event Carried State Transfer:
- Eventos carregam dados completos (não apenas IDs)
- Desacoplamento total entre serviços
- Consumidores não precisam consultar a origem
- Redução de chamadas síncronas
- Maior autonomia dos consumidores

#### `UseCases/`
Use Cases na Clean Architecture:
- Encapsulamento da lógica de negócio
- Independência de frameworks e infraestrutura
- Inversão de dependência (interfaces)
- Padrão Result para retorno de operações
- Single Responsibility por Use Case
- Exemplos: CreateUser, TransferMoney, ProcessOrder

#### `PersistencePatterns/`
Padrões de persistência de dados:
| Padrão | Descrição |
|--------|-----------|
| Repository | Abstração do acesso a dados |
| Unit of Work | Gerenciamento de transações |
| Identity Map | Cache de entidades carregadas |
| Data Mapper | Separação entre domínio e persistência |
| Active Record | Entidade com métodos de persistência |

#### `TransactionPattern/`
Padrão ExecuteInTransactionAsync:
- Encapsulamento de lógica transacional assíncrona
- Garantia de atomicidade (tudo ou nada)
- Commit/Rollback automático
- Tratamento centralizado de exceções
- Operações de banco de dados seguras

#### `TransactionScript/`
Transaction Script Pattern:
- Lógica de negócio organizada em procedimentos
- Ideal para operações CRUD simples
- Cada script lida com uma requisição
- Comparação com Domain Model
- Exemplos: TransferMoney, CreateInvoice, ProcessRefund

#### `ObjectCalisthenics/`
As 9 regras de Object Calisthenics para código limpo OO:
- API demonstrativa com implementações "Bad" vs "Good"
1. Apenas um nível de indentação por método
2. Não use a palavra-chave ELSE
3. Encapsule todos os primitivos e strings
4. First Class Collections
5. Um ponto por linha (Law of Demeter)
6. Não abrevie nomes
7. Mantenha todas as entidades pequenas
8. Máximo de duas variáveis de instância por classe
9. Sem getters/setters/properties públicos

---

### 🔄 Assincronia e Concorrência

#### `Asynchronous/`
Programação assíncrona com `async`/`await`:
- Task-based Asynchronous Pattern (TAP)
- Operações I/O não-bloqueantes

#### `AsyncTasksDemo/`
Demonstrações práticas de Tasks:
- Task.Run
- Task.WhenAll / Task.WhenAny
- Continuations

#### `Threads/`
Programação multithread:
- Thread Pool
- Sincronização
- Locks e Semaphores

#### `AtomicOperationsDemo/`
Operações atômicas e thread-safe:
- Interlocked operations
- Concurrent collections

#### `BackgroudWorker/`
Workers em background com `IHostedService`:
- `TimedHostedService` para tarefas agendadas
- Configuração via appsettings

#### `JobQueueDemo/`
Sistema de filas de trabalho background:
- Processamento assíncrono de jobs
- Gerenciamento de filas

---

### 📊 Algoritmos e Estruturas de Dados

#### `Linq/`
Language Integrated Query:
- Query Syntax vs Method Syntax
- Operadores de projeção, filtro, agregação
- LINQ to Objects

#### `SlidingWindows/`
Algoritmo de janela deslizante:
- Problemas de substring
- Rate limiting
- Análise de sequências

#### `RealWorldBubbleSort/`
Algoritmo de ordenação Bubble Sort com aplicações práticas.

#### `GraphTraversalDemo/`
Algoritmos de travessia de grafos:
- BFS (Breadth-First Search)
- DFS (Depth-First Search)
- Estrutura de grafo genérica

#### `LoadBalancingAlgorithms/`
Algoritmos de balanceamento de carga:
| Algoritmo | Descrição |
|-----------|-----------|
| Round Robin | Distribuição circular |
| Weighted Round Robin | Round Robin com pesos |
| Least Connections | Menor número de conexões |
| Random | Seleção aleatória |

#### `LogicalOperatorsDemo/`
Demonstração de operadores lógicos em C#.

#### `DictionaryMerge/`
Merge de dicionários com sincronização de notas fiscais.

---

### 🛠️ Utilitários e Transformação de Dados

#### `ClassToDTO/`
Mapeamento de classes para DTOs:
- Manual mapping
- AutoMapper
- Expression-based mapping

#### `ClassToXml/`
Serialização de objetos para XML.

#### `XmlBasics/`
Manipulação básica de XML:
- XmlDocument
- XDocument (LINQ to XML)
- XmlSerializer

#### `Serialization/`
Exemplos de serialização:
- JSON (System.Text.Json / Newtonsoft)
- XML
- Binary

#### `NPOIDemo/`
Geração de arquivos Office com NPOI:
- Criação de planilhas Excel (.xlsx)
- Geração de documentos Word (.docx)
- Formatação avançada (estilos, cores, fontes)
- Tabelas e cálculos
- Sem necessidade de Microsoft Office instalado

#### `PDFGenerator/`
Geração de documentos PDF:
- Criação de PDFs programaticamente
- Relatórios e documentos
- Templates personalizados

#### `CompressDecompress/`
Compressão e descompressão de dados:
- GZip
- Deflate
- Brotli

#### `Reflection/`
Reflexão em C#:
- Type inspection
- Dynamic invocation
- Attribute reading

#### `Events/`
Sistema de eventos em C#:
- EventHandler pattern
- Custom events
- Event aggregation

---

### ⚙️ Serviços e Infraestrutura

#### `ServiceRegistration/`
Registro de serviços em Dependency Injection:
- Transient, Scoped, Singleton
- Factory pattern
- Keyed services

#### `TaskManagement/`
Gerenciamento de tarefas e scheduling.

#### `SerilogExample/`
Logging estruturado com Serilog:
- Sinks (Console, File, Seq)
- Enrichers
- Structured logging

---

### 📦 SDKs e Bibliotecas

#### `MySimpleSdk/`
Exemplo de SDK customizado:
- Estrutura de projeto SDK
- Extensibility patterns
- Configuration

#### `UnifiedCacheSdk/`
SDK unificado para cache:
- Multiple provider support
- Abstraction layer
- Easy configuration

#### `ShareableUser/`
Biblioteca compartilhável:
- Middleware personalizado
- Services compartilhados

---

### 🎓 Aprendizado e Exemplos Básicos

#### `Course/`
Exemplos básicos de C# para iniciantes.

#### `BlockchainDemo/`
Demonstração de conceitos blockchain:
| Pasta | Descrição |
|-------|-----------|
| `Core/` | Lógica central do blockchain |
| `Models/` | Modelos de dados (Block, Transaction) |

---

### 🧪 Testes

#### `OrderRuleConsole/`
Console de regras de pedido para testes de lógica de negócio.

#### `OrderRuleConsole.Tests/`
Testes unitários com xUnit para regras de pedido.

---

## 🚀 Como Usar

### 📋 Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) ou superior
- [Visual Studio Code](https://code.visualstudio.com/) ou [Visual Studio](https://visualstudio.microsoft.com/)
- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/) (para projetos com Kafka/RabbitMQ/Redis)

### ⚡ Instalação e Execução

```bash
# Clonar o repositório
git clone https://github.com/LucasDiasJorge/CSharp-101.git
cd CSharp-101

# Restaurar dependências e compilar
dotnet build CSharp-101.sln

# Executar um projeto específico
cd [NomeDoProjeto]
dotnet run
```

### 🐳 Projetos com Docker

```bash
# Kafka
cd Kafka
docker-compose up -d

# Rodar producer
cd Send
dotnet run

# Rodar consumer
cd ../Receive
dotnet run
```

---

## 🛠️ Comandos Essenciais do .NET CLI

### 📌 Criando Projetos

```bash
# Console Application
dotnet new console -n MinhaApp --use-program-main

# Web API
dotnet new webapi -n MinhaApi

# Class Library
dotnet new classlib -n MinhaLib

# Worker Service
dotnet new worker -n MeuWorker

# xUnit Tests
dotnet new xunit -n MeusTests
```

### ✅ Templates Disponíveis

| Template | Comando | Descrição |
|----------|---------|-----------|
| Console | `dotnet new console` | 🖥️ Aplicação console |
| Web API | `dotnet new webapi` | 🌐 API REST |
| MVC | `dotnet new mvc` | 🏗️ Aplicação MVC |
| Blazor Server | `dotnet new blazorserver` | ⚡ Blazor server-side |
| Blazor WASM | `dotnet new blazorwasm` | 🌐 Blazor WebAssembly |
| Class Library | `dotnet new classlib` | 📚 Biblioteca de classes |
| Worker Service | `dotnet new worker` | ⚙️ Serviço background |
| gRPC | `dotnet new grpc` | 🔄 Serviço gRPC |
| xUnit | `dotnet new xunit` | 🧪 Testes xUnit |

### 🔧 Manipulação de Projetos

```bash
# Criar solução
dotnet new sln -n MinhaSolucao

# Adicionar projeto à solução
dotnet sln add MinhaApp/MinhaApp.csproj

# Listar projetos
dotnet sln list

# Restaurar, compilar e executar
dotnet restore
dotnet build
dotnet run
```

### 📦 Gerenciamento de Pacotes

```bash
# Adicionar pacote
dotnet add package Newtonsoft.Json

# Remover pacote
dotnet remove package Newtonsoft.Json

# Listar pacotes
dotnet list package

# Listar desatualizados
dotnet list package --outdated
```

### 💾 Entity Framework Core

```bash
# Instalar pacotes
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet ef migrations remove
dotnet ef migrations list
```

### 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Com verbosidade
dotnet test --verbosity normal

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### 🚀 Publicação

```bash
# Publicação básica
dotnet publish -c Release -o ./publish

# Self-contained para Windows
dotnet publish -c Release -r win-x64 --self-contained true

# Arquivo único
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

---

## 🏗️ Princípios SOLID

### 🔸 S — Single Responsibility Principle (SRP)

> **"Uma classe deve ter apenas uma razão para mudar"**

```csharp
// ✅ Cada classe com responsabilidade única
public class UserService { public void CreateUser(User user) { } }
public class EmailService { public void SendNotification(User user) { } }
public class LoggingService { public void LogActivity(string message) { } }
```

### 🔸 O — Open/Closed Principle (OCP)

> **"Aberto para extensão, fechado para modificação"**

```csharp
// ✅ Extensível via interface
public interface IPaymentMethod { void ProcessPayment(decimal amount); }
public class CreditCardPayment : IPaymentMethod { /* ... */ }
public class PayPalPayment : IPaymentMethod { /* ... */ }
```

### 🔸 L — Liskov Substitution Principle (LSP)

> **"Subclasses devem ser substituíveis por suas superclasses"**

```csharp
// ✅ Segregação correta de comportamentos
public abstract class Bird { public abstract void Move(); }
public interface IFlyingBird { void Fly(); }
public class Eagle : Bird, IFlyingBird { /* pode voar */ }
public class Penguin : Bird { /* não pode voar, mas se move */ }
```

### 🔸 I — Interface Segregation Principle (ISP)

> **"Clientes não devem depender de interfaces que não utilizam"**

```csharp
// ✅ Interfaces específicas
public interface IWorkable { void Work(); }
public interface IEatable { void Eat(); }
public class Robot : IWorkable { /* só trabalha */ }
public class Human : IWorkable, IEatable { /* trabalha e come */ }
```

### 🔸 D — Dependency Inversion Principle (DIP)

> **"Dependa de abstrações, não de implementações concretas"**

```csharp
// ✅ Injeção de dependência via interface
public class OrderService
{
    private readonly IRepository _repository;
    private readonly INotificationService _notificationService;

    public OrderService(IRepository repository, INotificationService notificationService)
    {
        _repository = repository;
        _notificationService = notificationService;
    }
}
```

---

## 🤝 Como Contribuir

1. **Fork** o repositório
2. Crie uma **branch** para sua feature (`git checkout -b feature/MinhaFeature`)
3. **Commit** suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. **Push** para a branch (`git push origin feature/MinhaFeature`)
5. Abra um **Pull Request**

### 📝 Diretrizes de Contribuição

- ✅ Siga as convenções de código C#
- ✅ Adicione documentação adequada
- ✅ Inclua testes para novas funcionalidades
- ✅ Mantenha o README atualizado
- ✅ Use commits semânticos

---

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">

**🚀 Feito com ❤️ para a comunidade de desenvolvedores C#/.NET**

[⭐ Deixe uma estrela](https://github.com/LucasDiasJorge/CSharp-101) • [🐛 Reporte um bug](https://github.com/LucasDiasJorge/CSharp-101/issues) • [💡 Solicite uma feature](https://github.com/LucasDiasJorge/CSharp-101/issues)

</div>
